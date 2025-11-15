namespace Domain;

public class Provincia
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;

    public ICollection<Localidad> Localidades { get; set; } = new List<Localidad>();
    public ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
