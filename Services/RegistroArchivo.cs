using System.Text;
using fvelezTA1.Models;

namespace fvelezTA1.Services;

/// <summary>Guarda cada inscripción en un archivo TXT dentro del almacenamiento privado de la app.</summary>
public static class RegistroArchivo
{
    public const string NombreArchivo = "inscripciones_uisrael.txt";

    public static string Ruta => Path.Combine(FileSystem.AppDataDirectory, NombreArchivo);

    public static async Task<string> GuardarAsync(Inscripcion i, string? usuario)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========== INSCRIPCIÓN UISRAEL · FASE A - GRADO ==========");
        sb.AppendLine($"Fecha de registro : {i.FechaRegistro:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"Registrado por    : {usuario}");
        sb.AppendLine("--- Datos personales ---");
        sb.AppendLine($"Identificación    : {i.TipoIdentificacion} {i.NumeroIdentificacion}");
        sb.AppendLine($"Apellido paterno  : {i.ApellidoPaterno}");
        sb.AppendLine($"Apellido materno  : {i.ApellidoMaterno}");
        sb.AppendLine($"Nombres completos : {i.Nombres}");
        sb.AppendLine($"Teléfono personal : {i.Telefono}");
        sb.AppendLine($"Correo personal   : {i.Correo}");
        sb.AppendLine("--- Datos académicos ---");
        sb.AppendLine($"Carrera           : {i.Carrera}");
        sb.AppendLine($"Modalidad         : {i.Modalidad}");
        sb.AppendLine($"Campus            : {i.Campus}");
        sb.AppendLine();

        await File.AppendAllTextAsync(Ruta, sb.ToString(), Encoding.UTF8);
        return Ruta;
    }

    public static async Task<string> LeerAsync() =>
        File.Exists(Ruta) ? await File.ReadAllTextAsync(Ruta, Encoding.UTF8) : string.Empty;

    public static int ContarRegistros(string contenido) =>
        string.IsNullOrEmpty(contenido) ? 0 : contenido.Split("========== INSCRIPCIÓN").Length - 1;
}
