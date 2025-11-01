namespace DTOs
{
    public class QuoteDTO
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD"; // o "ARS"
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = string.Empty;
    }
}
