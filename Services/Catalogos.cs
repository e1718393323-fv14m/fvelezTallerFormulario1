namespace fvelezTA1.Services;

/// <summary>
/// Listas de los selectores del formulario.
/// Ajusta carreras y campus a la oferta vigente publicada en el SIGE de la UISRAEL.
/// </summary>
public static class Catalogos
{
    public const string Cedula = "CÉDULA";
    public const string Ruc = "RUC";
    public const string Pasaporte = "PASAPORTE";

    public static readonly List<string> TiposIdentificacion = [Cedula, Ruc, Pasaporte];

    public static readonly List<string> Carreras =
    [
        "Software",
        "Tecnologías de la Información",
        "Administración de Empresas",
        "Contabilidad y Auditoría",
        "Diseño Gráfico",
        "Educación Básica",
        "Derecho",
        "Psicología"
    ];

    public static readonly List<string> Modalidades = ["Presencial", "Semipresencial", "En línea"];

    public static readonly List<string> Campus = ["Quito", "En línea"];
}
