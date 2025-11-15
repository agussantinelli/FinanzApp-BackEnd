namespace Domain;

public class CedearRatio
{
    public int Id { get; set; }

    public int CedearId { get; set; }
    public Activo Cedear { get; set; } = null!;          // Activo con símbolo tipo AAPL.BA

    public int UsAssetId { get; set; }
    public Activo UsAsset { get; set; } = null!;         // Activo con símbolo tipo AAPL

    public decimal Ratio { get; set; }                   // ej. 10 = 10 CEDEARs = 1 acción
}
