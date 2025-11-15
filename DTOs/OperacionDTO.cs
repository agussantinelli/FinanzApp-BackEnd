namespace DTOs;

public class OperacionDTO
{
    public int Id { get; set; }

    public int PersonaId { get; set; }
    public int ActivoId { get; set; }

    public string Tipo { get; set; } = string.Empty;      // "Compra" / "Venta"
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string MonedaOperacion { get; set; } = "ARS";
    public DateTime FechaOperacion { get; set; }
    public decimal? Comision { get; set; }

    public string ActivoSymbol { get; set; } = string.Empty;
    public string ActivoNombre { get; set; } = string.Empty;
    public string PersonaNombreCompleto { get; set; } = string.Empty;
}
