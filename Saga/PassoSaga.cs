namespace PvPmo.Saga
{
    // Interfaccia per un passo della Saga
    
    public interface IPassoSaga
    {
        Task Esegui();
        Task Compensa();
    }

    // --- PASSO 1: PAGAMENTO ---

    public partial class CaricaTabelle : IPassoSaga
    {
        private readonly ServizioTabelle _servizio;
        private readonly string _nTabella;
        private readonly string _nDb;

        public CaricaTabelle(ServizioTabelle servizio, string nTabella, string nDb)
        {
            _servizio = servizio;
            _nTabella = nTabella;
            _nDb = nDb;
        }

        public async Task Esegui() => await _servizio.CaricaTabelleDb(_nTabella, _nDb);
        public async Task Compensa() => await _servizio.RestoreTabelleDb(_nTabella, _nDb);

    }
    public class PassoPagamento : IPassoSaga
    {
        private readonly ServizioPagamenti _servizio;
        private readonly string _idUtente;
        private readonly decimal _importo;

        public PassoPagamento(ServizioPagamenti servizio, string idUtente, decimal importo)
        {
            _servizio = servizio;
            _idUtente = idUtente;
            _importo = importo;
        }

        public async Task Esegui() => await _servizio.ProcessaPagamento(_idUtente, _importo);
        public async Task Compensa() => await _servizio.RimborsaPagamento(_idUtente, _importo);
    }

    // --- PASSO 2: INVENTARIO ---
    public class PassoInventario : IPassoSaga
    {
        private readonly ServizioInventario _servizio;
        private readonly string _idProdotto;
        private readonly int _quantita;

        public PassoInventario(ServizioInventario servizio, string idProdotto, int quantita)
        {
            _servizio = servizio;
            _idProdotto = idProdotto;
            _quantita = quantita;
        }

        public async Task Esegui() => await _servizio.ScalaProdotto(_idProdotto, _quantita);
        public async Task Compensa() => await _servizio.RipristinaProdotto(_idProdotto, _quantita);
    }

    // --- PASSO 3: SPEDIZIONE ---
    public class PassoSpedizione : IPassoSaga
    {
        private readonly ServizioSpedizioni _servizio;
        private readonly string _idUtente;
        private readonly string _idProdotto;

        public PassoSpedizione(ServizioSpedizioni servizio, string idUtente, string idProdotto)
        {
            _servizio = servizio;
            _idUtente = idUtente;
            _idProdotto = idProdotto;
        }

        public async Task Esegui() => await _servizio.CreaSpedizione(_idUtente, _idProdotto);
        public async Task Compensa() => await _servizio.AnnullaSpedizione(_idUtente, _idProdotto);
    }
}
