namespace PvPmo.Saga
{
    // File: OrchestratoreSagaOrdine.cs

    public partial class OrchestratoreSagaOrdine
    {
        private readonly ServizioPagamenti _servizioPagamenti;
        private readonly ServizioInventario _servizioInventario;
        private readonly ServizioSpedizioni _servizioSpedizioni;

        private readonly ServizioTabelle _servizioTabelle;

        public OrchestratoreSagaOrdine(ServizioTabelle tab)
        {
            _servizioTabelle = tab;
            
        }

        public OrchestratoreSagaOrdine(ServizioPagamenti pag, ServizioInventario inv, ServizioSpedizioni sped)
        {
            _servizioPagamenti = pag;
            _servizioInventario = inv;
            _servizioSpedizioni = sped;
        }

        public async Task<bool> EseguiSagaAcquisto(string idUtente, string idProdotto, int quantita, decimal importo)
        {
            var passiCompletati = new Stack<IPassoSaga>();

            try
            {
                // --- ESECUZIONE PASSO 1: PAGAMENTO ---
                var passoPagamento = new PassoPagamento(_servizioPagamenti, idUtente, importo);
                Console.WriteLine("-> ESEGUO: Pagamento...");
                await passoPagamento.Esegui();
                passiCompletati.Push(passoPagamento);
                Console.WriteLine("   OK: Pagamento completato.");

                // --- ESECUZIONE PASSO 2: INVENTARIO ---
                var passoInventario = new PassoInventario(_servizioInventario, idProdotto, quantita);
                Console.WriteLine("-> ESEGUO: Inventario...");
                await passoInventario.Esegui();
                passiCompletati.Push(passoInventario);
                Console.WriteLine("   OK: Inventario aggiornato.");

                // --- ESECUZIONE PASSO 3: SPEDIZIONE ---
                var passoSpedizione = new PassoSpedizione(_servizioSpedizioni, idUtente, idProdotto);
                Console.WriteLine("-> ESEGUO: Spedizione...");
                await passoSpedizione.Esegui();
                passiCompletati.Push(passoSpedizione);
                Console.WriteLine("   OK: Spedizione creata.");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSAGA COMPLETATA CON SUCCESSO! Ordine confermato.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n!!! ERRORE nella Saga: {ex.Message}. Avvio compensazione... !!!");
                Console.ResetColor();

                // --- COMPENSAZIONE ---
                await AnnullaPassiPrecedenti(passiCompletati);
                return false;
            }
        }

        private async Task AnnullaPassiPrecedenti(Stack<IPassoSaga> passi)
        {
            Console.WriteLine("\n--- Inizio Rollback Logico ---");
            // La pila assicura che compensiamo in ordine inverso
            foreach (var passo in passi)
            {
                await passo.Compensa();
            }
            Console.WriteLine("--- Rollback Logico Completato ---");
        }
    }
}
