using System.Globalization;

namespace WindowsOptimizer.Core.Formatting;

public static class ByteFormatter
{
    private static readonly string[] Units = ["Б", "КБ", "МБ", "ГБ", "ТБ"];

    public static string ToHuman(long bytes)
    {
        if (bytes < 0)
        {
            return "0 Б";
        }

        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < Units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? $"{bytes} {Units[unit]}"
            : string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", value, Units[unit]);
    }
}
