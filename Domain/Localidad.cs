namespace Domain;

public class Localidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int ProvinciaId { get; set; }
    public Provincia Provincia { get; set; } = null!;

    public ICollection<Persona> PersonasResidencia { get; set; } = new List<Persona>();
}
