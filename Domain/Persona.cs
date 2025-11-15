namespace Domain;

public class Persona
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public DateTime FechaNacimiento { get; set; }

    public bool EsResidenteArgentina { get; set; }

    public RolPersona Rol { get; set; } = RolPersona.Inversor;

    public int NacionalidadId { get; set; }
    public Pais Nacionalidad { get; set; } = null!;

    public int? PaisResidenciaId { get; set; }
    public Pais? PaisResidencia { get; set; }

    public int? LocalidadResidenciaId { get; set; }
    public Localidad? LocalidadResidencia { get; set; }

    public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
}
