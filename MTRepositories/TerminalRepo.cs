using MTsocketAPI.MT5;

namespace Sati_React.Server.MTRepositories;

public class TerminalRepo
{
    private readonly Terminal _terminal;

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
    public void Connect() => _terminal.Connect();
    
    public void TrackPrices(List<string> symbols) => _terminal.TrackPrices(symbols);

    public List<Rates> GetPriceHistory(string symbol, TimeFrame timeFrame, DateTime startDate, DateTime endDate) => _terminal.PriceHistory(symbol, timeFrame, startDate, endDate);
}