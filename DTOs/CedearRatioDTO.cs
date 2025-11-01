namespace DTOs
{
    public class CedearRatioDTO
    {
        public string CedearSymbol { get; set; } = string.Empty; // p.ej. AAPL.BA
        public string UsSymbol { get; set; } = string.Empty;     // p.ej. AAPL
        public decimal Ratio { get; set; }                       // ej. 10 = 10 CEDEARs = 1 acción
    }
}
