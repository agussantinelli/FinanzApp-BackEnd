namespace DTOs;

public class CotizacionDTO
{
    public int Id { get; set; }
    public int ActivoId { get; set; }

    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime TimestampUtc { get; set; }
    public string? Source { get; set; }

    public string ActivoSymbol { get; set; } = string.Empty;
}
