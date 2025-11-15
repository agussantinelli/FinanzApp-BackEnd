namespace Domain;

public class Pais
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string CodigoIso2 { get; set; } = string.Empty;   // AR, BR, US...
    public string CodigoIso3 { get; set; } = string.Empty;   // ARG, BRA, USA...
    public bool EsArgentina { get; set; }                    

    public ICollection<Provincia> Provincias { get; set; } = new List<Provincia>();

    public ICollection<Persona> PersonasResidencia { get; set; } = new List<Persona>();
    public ICollection<Persona> PersonasNacionalidad { get; set; } = new List<Persona>();
}
