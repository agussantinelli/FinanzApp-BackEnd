namespace Domain;

public class Operacion
{
    public int Id { get; set; }

    public int PersonaId { get; set; }
    public Persona Persona { get; set; } = null!;

    public int ActivoId { get; set; }
    public Activo Activo { get; set; } = null!;

    public TipoOperacion Tipo { get; set; }               // Compra / Venta
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string MonedaOperacion { get; set; } = "ARS";  // 'ARS', 'USD', etc.
    public DateTime FechaOperacion { get; set; }
    public decimal? Comision { get; set; }

    public decimal MontoTotal => Cantidad * PrecioUnitario + (Comision ?? 0m);
}
