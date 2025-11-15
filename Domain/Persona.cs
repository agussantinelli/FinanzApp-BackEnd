namespace Domain;

public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
}
