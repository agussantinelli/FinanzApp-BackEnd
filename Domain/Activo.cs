namespace Domain;

public class Activo
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;   // AAPL, AAPL.BA, BTCUSDT, etc.
    public string Nombre { get; set; } = string.Empty;
    public TipoActivo Tipo { get; set; }
    public string MonedaBase { get; set; } = "USD";      // 'USD', 'ARS', ...
    public bool EsLocal { get; set; }                    // cotiza en mercado local (BYMA/CEDEAR)?

    public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
    public ICollection<Cotizacion> Cotizaciones { get; set; } = new List<Cotizacion>();

    public CedearRatio? CedearRatioCedear { get; set; }   // si este activo es el CEDEAR
    public CedearRatio? CedearRatioUsAsset { get; set; }  // si este activo es la acción USA
}
