namespace DTOs
{
    public class DualQuoteDTO
    {
        public string LocalSymbol { get; set; } = string.Empty; // p.ej. GGAL.BA o AAPL.BA
        public decimal LocalPriceARS { get; set; }
        public string UsSymbol { get; set; } = string.Empty;     // p.ej. GGAL o AAPL
        public decimal UsPriceUSD { get; set; }

        // Para CEDEARs: cantidad de CEDEARs que representan 1 acción USA
        public decimal? CedearRatio { get; set; } // ej. 10m/1 (guardar como 10)
        public decimal UsedDollarRate { get; set; } // CCL/MEP usado
        public string DollarRateName { get; set; } = "CCL";

        // Conversión cruzada
        public decimal LocalPriceUSD => UsedDollarRate == 0 ? 0 : Math.Round(LocalPriceARS / UsedDollarRate, 2);
        public decimal UsPriceARS => Math.Round(UsPriceUSD * UsedDollarRate, 2);

        // Para CEDEAR: “paridad” teórica (precio USA * CCL / ratio)
        public decimal? TheoreticalCedearARS =>
            (CedearRatio.HasValue && CedearRatio.Value > 0)
              ? Math.Round((UsPriceUSD * UsedDollarRate) / CedearRatio.Value, 2)
              : null;
    }
}
