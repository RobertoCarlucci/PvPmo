namespace PvPmo.Saga
{
    // File: Servizi.cs
    public partial class ServizioTabelle
    {
        public async Task CaricaTabelleDb(string nTabella, string nDb)
        {
            
        }
        public async Task RestoreTabelleDb(string nTabella, string nDb)
        {

        }
    }
    public class ServizioPagamenti
    {
        public async Task ProcessaPagamento(string idUtente, decimal importo)
        {
            await Task.Delay(100); // Simula una chiamata di rete
            Console.WriteLine($"   > Pagamento di {importo:C} per l'utente {idUtente} processato.");
        }

        public async Task RimborsaPagamento(string idUtente, decimal importo)
        {
            await Task.Delay(100);
            Console.WriteLine($"   > Rimborso di {importo:C} per l'utente {idUtente} effettuato.");
        }
    }

    public class ServizioInventario
    {
        public async Task ScalaProdotto(string idProdotto, int quantita)
        {
            await Task.Delay(100);
            Console.WriteLine($"   > Scalata quantità di {quantita} per il prodotto {idProdotto} dal magazzino.");
        }

        public async Task RipristinaProdotto(string idProdotto, int quantita)
        {
            await Task.Delay(100);
            Console.WriteLine($"   > Ripristinata quantità di {quantita} per il prodotto {idProdotto} in magazzino.");
        }
    }

    public class ServizioSpedizioni
    {
        public async Task CreaSpedizione(string idUtente, string idProdotto)
        {
            await Task.Delay(100);
            // Simuliamo un possibile fallimento nel sistema di spedizioni
            if (new Random().Next(0, 3) == 0) // Fallisce circa il 33% delle volte
            {
                throw new InvalidOperationException("Errore nel sistema di logistica, impossibile creare la spedizione.");
            }
            Console.WriteLine($"   > Spedizione creata per l'utente {idUtente}, prodotto {idProdotto}.");
        }

        public async Task AnnullaSpedizione(string idUtente, string idProdotto)
        {
            await Task.Delay(100);
            Console.WriteLine($"   > Spedizione annullata per l'utente {idUtente}, prodotto {idProdotto}.");
        }
    }
}
