using MTsocketAPI.MT5;

namespace Sati_Net_Last.API.MTRepositories.Interfaces;

public interface IMTRepo
{
    public Task ConnectToMetaTrader();
    public List<string> GetSymbolList();
    public List<Rates> GetPriceHistory();
    Task<List<Rates>> GetDatePriceHistoryAsync(DateTime dateFilter, string symbolStr, int wamPeriod);
    public byte[] GetDatePriceHistoryExcel(DateTime dateFilter, string symbolStr, int wamPeriod);
    public byte[] GetDatePriceHistoryExcelWithAlgorithm(List<Rates> rates, string symbolStr, DateTime dateFilter, int wamPeriod);
    // public List<Rates> GetDatePriceHistory(DateTime dateFilter, string symbolStr);
    public void DisconnectFromMetaTrader();
    public void Mt5_OnPrice(object? sender, Quote e);
}
