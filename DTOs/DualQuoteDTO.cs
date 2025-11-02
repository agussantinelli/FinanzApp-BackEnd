namespace DTOs
{
    public class DualQuoteDTO
    {
        public string LocalSymbol { get; set; } = string.Empty; // p.ej. GGAL.BA o AAPL.BA
        public decimal LocalPriceARS { get; set; }
        public string UsSymbol { get; set; } = string.Empty;     // p.ej. GGAL o AAPL
        public decimal UsPriceUSD { get; set; }

        public decimal? CedearRatio { get; set; } // ej. 10 → 10 CEDEARs = 1 acción
        public decimal UsedDollarRate { get; set; } // CCL/MEP
        public string DollarRateName { get; set; } = "CCL";

        public decimal LocalPriceUSD => UsedDollarRate == 0 ? 0 : Math.Round(LocalPriceARS / UsedDollarRate, 2);
        public decimal UsPriceARS => Math.Round(UsPriceUSD * UsedDollarRate, 2);

        public decimal? TheoreticalCedearARS =>
            (CedearRatio.HasValue && CedearRatio.Value > 0)
              ? Math.Round((UsPriceUSD * UsedDollarRate) / CedearRatio.Value, 2)
              : null;
    }
}
