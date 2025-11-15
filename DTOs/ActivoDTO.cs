namespace DTOs;

public class ActivoDTO
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public byte Tipo { get; set; }            // mapea con enum TipoActivo (0,1,2,3)
    public string MonedaBase { get; set; } = "USD";
    public bool EsLocal { get; set; }
}
