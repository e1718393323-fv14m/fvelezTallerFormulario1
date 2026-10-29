namespace fvelezTA1.Models;

/// <summary>Datos del formulario de inscripción (Fase A - Grado).</summary>
public sealed class Inscripcion
{
    public string TipoIdentificacion { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string ApellidoPaterno { get; init; } = string.Empty;
    public string ApellidoMaterno { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string Carrera { get; init; } = string.Empty;
    public string Modalidad { get; init; } = string.Empty;
    public string Campus { get; init; } = string.Empty;
    public DateTime FechaRegistro { get; init; } = DateTime.Now;
}
