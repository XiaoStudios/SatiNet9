using MTsocketAPI.MT5;

namespace Sati_Net_Last.API.Services;

public static class HistoryTimeFrameParser
{
    public static bool TryParse(string? value, out TimeFrame timeFrame)
    {
        switch (value?.Trim().ToUpperInvariant())
        {
            case "M1":
                timeFrame = TimeFrame.PERIOD_M1;
                return true;
            case "M2":
                timeFrame = TimeFrame.PERIOD_M2;
                return true;
            case "M3":
                timeFrame = TimeFrame.PERIOD_M3;
                return true;
            case "M4":
                timeFrame = TimeFrame.PERIOD_M4;
                return true;
            case "M5":
                timeFrame = TimeFrame.PERIOD_M5;
                return true;
            case "M6":
                timeFrame = TimeFrame.PERIOD_M6;
                return true;
            default:
                timeFrame = default;
                return false;
        }
    }
}