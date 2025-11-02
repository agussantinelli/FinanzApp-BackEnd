namespace Services;

public static class CedearsRatios
{
    private static readonly Dictionary<string, decimal> _ratios =
        new(StringComparer.OrdinalIgnoreCase)
        {
        };

    public static bool TryGetRatio(string cedearSymbol, out decimal ratio)
        => _ratios.TryGetValue(cedearSymbol, out ratio);

    public static IReadOnlyDictionary<string, decimal> All => _ratios;
}
