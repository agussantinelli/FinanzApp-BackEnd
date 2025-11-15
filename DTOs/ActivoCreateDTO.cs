namespace DTOs;

public class ActivoCreateDTO
{
    public string Symbol { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public byte Tipo { get; set; }            // 0=AccionUsa, 1=Cedear, 2=Cripto, 3=Bono
    public string MonedaBase { get; set; } = "USD";
    public bool EsLocal { get; set; }
}
