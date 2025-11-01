namespace DTOs
{
    public class CryptoTopDTO
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;      
        public string Symbol { get; set; } = string.Empty;    
        public decimal PriceUsd { get; set; }
        public string Source { get; set; } = "CoinGecko";
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
