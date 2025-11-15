namespace Domain;

public class Cotizacion
{
    public int Id { get; set; }

    public int ActivoId { get; set; }
    public Activo Activo { get; set; } = null!;

    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "USD";          // 'USD', 'ARS', etc.
    public DateTime TimestampUtc { get; set; }
    public string? Source { get; set; }                  // Binance, Yahoo, etc.
}
