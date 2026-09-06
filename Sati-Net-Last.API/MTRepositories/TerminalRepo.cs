using MTsocketAPI.MT5;

namespace Sati_Net_Last.API.MTRepositories;

public class TerminalRepo
{
    private readonly Terminal _terminal;
    public bool IsConnected { get; private set; }

    public TerminalRepo(Terminal terminal)
    {
        _terminal = terminal;
    }

    public Terminal GetTerminal()
    {
        return _terminal;
    }

    // Puedes agregar aquí métodos utilitarios para exponer funcionalidad de Terminal si lo deseas
    // Ejemplo:
    public bool Connect()
    {
        IsConnected = _terminal.Connect();
        return IsConnected;
    }
    
    public void TrackPrices(List<string> symbols) => _terminal.TrackPrices(symbols);

    public Quote GetQuote(string symbol) => _terminal.GetQuote(symbol);

    public List<Rates> GetPriceHistory(string symbol, TimeFrame timeFrame, DateTime startDate, DateTime endDate) => _terminal.PriceHistory(symbol, timeFrame, startDate, endDate);
}
