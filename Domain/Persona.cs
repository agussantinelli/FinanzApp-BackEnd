namespace Domain;

public class Persona
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public DateTime FechaNac { get; set; }        


    public int PaisResidenciaId { get; set; }
    public Pais PaisResidencia { get; set; } = null!;

    // Nacionalidad
    public int PaisNacionalidadId { get; set; }
    public Pais PaisNacionalidad { get; set; } = null!;

    // Provincia / Localidad (solo obligatorio si resid. = Argentina → se valida en app)
    public int? ProvinciaId { get; set; }
    public Provincia? Provincia { get; set; }

    public int? LocalidadId { get; set; }
    public Localidad? Localidad { get; set; }

    // Navegación existente
    public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();

    // Propiedad de conveniencia (no mapeada) para la lógica de UI
    public bool EsResidenteArgentina => PaisResidencia.EsArgentina;
}
