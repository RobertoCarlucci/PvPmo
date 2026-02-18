using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using ExcelCell = DocumentFormat.OpenXml.Spreadsheet.Cell;

namespace PvPmo.Services
{
    public static class ExcelImporter
    {
        // Importa un file Excel in un DataTable
        
        /// <param name="filePath">Percorso del file Excel (.xlsx)</param>
        /// <param name="sheetName">Nome del foglio (opzionale, usa il primo se null)</param>
        /// <param name="hasHeader">Se true, la prima riga viene usata come intestazione</param>
        /// <returns>DataTable con i dati importati</returns>
        public static DataTable ImportExcelToDataTable(string filePath, string? sheetName = null, bool hasHeader = true)
        {
            // Validazione del percorso file
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"Il file Excel '{filePath}' non esiste.", filePath);
            }

            var dataTable = new DataTable();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart? workbookPart = document.WorkbookPart;
                if (workbookPart == null)
                    throw new InvalidOperationException("Il file Excel non contiene dati validi.");

                // Trova il foglio specificato o usa il primo
                Sheet? sheet;
                if (string.IsNullOrEmpty(sheetName))
                {
                    sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                }
                else
                {
                    sheet = workbookPart.Workbook.Descendants<Sheet>()
                        .FirstOrDefault(s => s.Name == sheetName);
                }

                if (sheet == null)
                    throw new InvalidOperationException($"Foglio '{sheetName ?? "primo"}' non trovato.");

                WorksheetPart? worksheetPart = (WorksheetPart?)workbookPart.GetPartById(sheet.Id!);
                SheetData? sheetData = worksheetPart?.Worksheet?.Elements<SheetData>().FirstOrDefault();

                if (sheetData == null)
                    throw new InvalidOperationException("Il foglio non contiene dati.");

                var rows = sheetData.Elements<Row>().ToList();
                if (rows.Count == 0)
                    return dataTable;

                // Determina il numero di colonne
                int columnCount = rows.Max(row => row.Elements<ExcelCell>().Count());

                // Crea le colonne
                if (hasHeader && rows.Count > 0)
                {
                    var headerRow = rows[0];
                    foreach (ExcelCell cell in headerRow.Elements<ExcelCell>())
                    {
                        string? columnName = GetCellValue(cell, workbookPart);
                        dataTable.Columns.Add(string.IsNullOrEmpty(columnName) ? $"Column{dataTable.Columns.Count + 1}" : columnName);
                    }
                    // Rimuovi la riga di intestazione
                    rows.RemoveAt(0);
                }
                else
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        dataTable.Columns.Add($"Column{i + 1}");
                    }
                }

                // Inferisci i tipi di colonna dai dati
                InferColumnTypes(dataTable, rows, workbookPart);

                // Aggiungi i dati
                foreach (Row row in rows)
                {
                    DataRow dataRow = dataTable.NewRow();
                    var cells = row.Elements<ExcelCell>().ToList();

                    int columnIndex = 0;
                    foreach (ExcelCell cell in cells)
                    {
                        // Gestisce celle vuote
                        int cellColumnIndex = GetColumnIndex(cell.CellReference?.Value ?? "A1");
                        
                        while (columnIndex < cellColumnIndex && columnIndex < dataTable.Columns.Count)
                        {
                            dataRow[columnIndex] = DBNull.Value;
                            columnIndex++;
                        }

                        if (columnIndex < dataTable.Columns.Count)
                        {
                            dataRow[columnIndex] = GetTypedCellValue(cell, workbookPart, dataTable.Columns[columnIndex].DataType);
                            columnIndex++;
                        }
                    }

                    // Riempie le celle vuote rimanenti
                    while (columnIndex < dataTable.Columns.Count)
                    {
                        dataRow[columnIndex] = DBNull.Value;
                        columnIndex++;
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }
        
        // Importa un file Excel in modo asincrono
        
        public static Task<DataTable> ImportExcelToDataTableAsync(string filePath, string? sheetName = null, bool hasHeader = true)
        {
            return Task.Run(() => ImportExcelToDataTable(filePath, sheetName, hasHeader));
        }

        // Ottiene il valore di una cella, gestendo stringhe condivise

        private static string? GetCellValue(ExcelCell cell, WorkbookPart workbookPart)
        {
            // Priorità a CellValue.Text per ottenere i risultati delle formule calcolate
            string? value = cell.CellValue?.Text ?? cell.InnerText;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                SharedStringTablePart? stringTablePart = workbookPart.SharedStringTablePart;
                if (stringTablePart != null && int.TryParse(value, out int index))
                {
                    return stringTablePart.SharedStringTable.Elements<SharedStringItem>()
                        .ElementAt(index).InnerText;
                }
            }

            return value;
        }
       
        // Ottiene l'indice numerico della colonna da un riferimento di cella (es. "A1" -> 0, "B1" -> 1)
        
        private static int GetColumnIndex(string cellReference)
        {
            string columnName = new string(cellReference.Where(char.IsLetter).ToArray());
            int columnIndex = 0;
            int factor = 1;

            for (int i = columnName.Length - 1; i >= 0; i--)
            {
                columnIndex += (columnName[i] - 'A' + 1) * factor;
                factor *= 26;
            }

            return columnIndex - 1;
        }
        
        // Ottiene l'elenco dei nomi dei fogli in un file Excel
       
        public static List<string> GetSheetNames(string filePath)
        {
            var sheetNames = new List<string>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart? workbookPart = document.WorkbookPart;
                if (workbookPart != null)
                {
                    sheetNames.AddRange(
                        workbookPart.Workbook.Descendants<Sheet>()
                            .Select(s => s.Name?.Value ?? string.Empty)
                            .Where(name => !string.IsNullOrEmpty(name))
                    );
                }
            }

            return sheetNames;
        }

        private static void InferColumnTypes(DataTable dataTable, List<Row> rows, WorkbookPart workbookPart)
        {
            int sampleSize = Math.Min(100, rows.Count); // Analizza le prime 100 righe
            
            for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
            {
                Type? inferredType = null;
                int dateCount = 0, numberCount = 0, boolCount = 0, stringCount = 0;

                foreach (var row in rows.Take(sampleSize))
                {
                    var cells = row.Elements<ExcelCell>().ToList();
                    ExcelCell? targetCell = null;

                    foreach (ExcelCell cell in cells)
                    {
                        int cellColumnIndex = GetColumnIndex(cell.CellReference?.Value ?? "A1");
                        if (cellColumnIndex == colIndex)
                        {
                            targetCell = cell;
                            break;
                        }
                    }

                    if (targetCell == null || string.IsNullOrWhiteSpace(targetCell.InnerText))
                        continue;

                    // Determina il tipo dalla cella in base al DataType di Excel
                    if (targetCell.DataType != null)
                    {
                        // Gestione esplicita dei tipi Excel
                        var cellType = targetCell.DataType.Value;
                        if (cellType.Equals(CellValues.Boolean))
                        {
                            boolCount++;
                        }
                        else if (cellType.Equals(CellValues.Number))
                        {
                            // Verifica se è una data
                            if (targetCell.StyleIndex != null && 
                                IsDateTimeFormat(workbookPart, (int)targetCell.StyleIndex.Value))
                            {
                                dateCount++;
                            }
                            else if (double.TryParse(targetCell.CellValue?.Text, 
                                System.Globalization.NumberStyles.Any, 
                                System.Globalization.CultureInfo.InvariantCulture, out _))
                            {
                                numberCount++;
                            }
                            else
                            {
                                stringCount++;
                            }
                        }
                        else if (cellType.Equals(CellValues.SharedString) ||
                                 cellType.Equals(CellValues.InlineString) ||
                                 cellType.Equals(CellValues.String))
                        {
                            // Esplicitamente stringhe
                            stringCount++;
                        }
                        else
                        {
                            // Altri tipi non comuni, trattati come stringhe
                            stringCount++;
                        }
                    }
                    else
                    {
                        // DataType == null: cella senza tipo esplicito
                        // Può essere numero o testo in base al contenuto
                        
                        // Prima verifica se è una data (basandosi sul formato)
                        if (targetCell.StyleIndex != null && 
                            IsDateTimeFormat(workbookPart, (int)targetCell.StyleIndex.Value))
                        {
                            dateCount++;
                        }
                        // Poi verifica se può essere un numero
                        else if (!string.IsNullOrEmpty(targetCell.CellValue?.Text) &&
                                 double.TryParse(targetCell.CellValue.Text, 
                                     System.Globalization.NumberStyles.Any, 
                                     System.Globalization.CultureInfo.InvariantCulture, out _))
                        {
                            numberCount++;
                        }
                        else
                        {
                            // Non è parsabile come numero, quindi è testo
                            stringCount++;
                        }
                    }
                }

                // Determina il tipo prevalente
                if (dateCount > 0 && dateCount >= numberCount && dateCount >= boolCount)
                {
                    inferredType = typeof(DateTime);
                }
                else if (numberCount > 0 && numberCount >= stringCount)
                {
                    inferredType = typeof(double);
                }
                else if (boolCount > 0 && boolCount >= stringCount)
                {
                    inferredType = typeof(bool);
                }
                else
                {
                    inferredType = typeof(string);
                }

                // Imposta il tipo di colonna
                if (inferredType != typeof(string))
                {
                    dataTable.Columns[colIndex].DataType = inferredType;
                }
            }
        }

        private static bool IsDateTimeFormat(WorkbookPart workbookPart, int styleIndex)
        {
            var styleSheet = workbookPart.WorkbookStylesPart?.Stylesheet;
            if (styleSheet == null) return false;

            var cellFormats = styleSheet.CellFormats;
            if (cellFormats == null || styleIndex >= cellFormats.Count()) return false;

            var cellFormat = cellFormats.ElementAt(styleIndex) as CellFormat;
            if (cellFormat?.NumberFormatId == null) return false;

            uint numFmtId = cellFormat.NumberFormatId.Value;

            // ID formati data standard Excel (14-22, 45-47)
            if ((numFmtId >= 14 && numFmtId <= 22) || (numFmtId >= 45 && numFmtId <= 47))
                return true;

            // Verifica formati personalizzati
            var numberingFormats = styleSheet.NumberingFormats;
            if (numberingFormats != null)
            {
                var numberFormat = numberingFormats.Elements<NumberingFormat>()
                    .FirstOrDefault(nf => nf.NumberFormatId?.Value == numFmtId);
                
                if (numberFormat?.FormatCode?.Value != null)
                {
                    string formatCode = numberFormat.FormatCode.Value.ToLower();
                    return formatCode.Contains("yyyy") || formatCode.Contains("dd") || 
                           formatCode.Contains("mm") || formatCode.Contains("hh");
                }
            }

            return false;
        }

        private static object GetTypedCellValue(ExcelCell cell, WorkbookPart workbookPart, Type targetType)
        {
            string? rawValue = GetCellValue(cell, workbookPart);
            
            if (string.IsNullOrWhiteSpace(rawValue))
                return DBNull.Value;

            try
            {
                if (targetType == typeof(DateTime))
                {
                    // Excel memorizza le date come numeri seriali
                    if (double.TryParse(rawValue, System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out double serialDate))
                    {
                        return DateTime.FromOADate(serialDate);
                    }
                    // Tenta parsing diretto
                    if (DateTime.TryParse(rawValue, out DateTime dateValue))
                    {
                        return dateValue;
                    }
                }
                else if (targetType == typeof(double))
                {
                    if (double.TryParse(rawValue, System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out double numValue))
                    {
                        return numValue;
                    }
                }
                else if (targetType == typeof(bool))
                {
                    if (bool.TryParse(rawValue, out bool boolValue))
                    {
                        return boolValue;
                    }
                    // Excel può usare 0/1 per booleani
                    if (rawValue == "1" || rawValue.Equals("true", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (rawValue == "0" || rawValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }
            catch
            {
                // In caso di errore di conversione, restituisce la stringa
            }

            return rawValue;
        }
    }
}