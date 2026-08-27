namespace MTsocketAPI.MT5;

public static class TimeFrameExtension
{
    public static TimeSpan ToTimeSpan(this TimeFrame timeFrame)
    {
        return timeFrame switch
        {
            TimeFrame.PERIOD_M1 => TimeSpan.FromMinutes(1),
            TimeFrame.PERIOD_M2 => TimeSpan.FromMinutes(2),
            TimeFrame.PERIOD_M3 => TimeSpan.FromMinutes(3),
            TimeFrame.PERIOD_M4 => TimeSpan.FromMinutes(4),
            TimeFrame.PERIOD_M5 => TimeSpan.FromMinutes(5),
            TimeFrame.PERIOD_M6 => TimeSpan.FromMinutes(6),
            TimeFrame.PERIOD_M10 => TimeSpan.FromMinutes(10),
            TimeFrame.PERIOD_M12 => TimeSpan.FromMinutes(12),
            TimeFrame.PERIOD_M15 => TimeSpan.FromMinutes(15),
            TimeFrame.PERIOD_M20 => TimeSpan.FromMinutes(20),
            TimeFrame.PERIOD_M30 => TimeSpan.FromMinutes(30),
            TimeFrame.PERIOD_H1 => TimeSpan.FromHours(1),
            TimeFrame.PERIOD_H2 => TimeSpan.FromHours(2),
            TimeFrame.PERIOD_H3 => TimeSpan.FromHours(3),
            TimeFrame.PERIOD_H4 => TimeSpan.FromHours(4),
            TimeFrame.PERIOD_H6 => TimeSpan.FromHours(6),
            TimeFrame.PERIOD_H8 => TimeSpan.FromHours(8),
            TimeFrame.PERIOD_H12 => TimeSpan.FromHours(12),
            TimeFrame.PERIOD_D1 => TimeSpan.FromDays(1),
            TimeFrame.PERIOD_W1 => TimeSpan.FromDays(7),
            TimeFrame.PERIOD_MN1 => TimeSpan.FromDays(30),
            _ => TimeSpan.FromMinutes(1)
        };
    }
}