using System.Security.Cryptography;
using System.Text;

namespace fvelezTA1.Services;

/// <summary>
/// Login básico con protección de la contraseña:
/// - La contraseña nunca se guarda en texto plano: solo su hash SHA-256 con sal.
/// - La comparación se hace en tiempo constante (FixedTimeEquals).
/// - Tras 3 intentos fallidos, el ingreso se bloquea 30 segundos.
/// </summary>
public static class Seguridad
{
    public const int IntentosMaximos = 3;
    public static readonly TimeSpan TiempoBloqueo = TimeSpan.FromSeconds(30);

    private const string UsuarioRegistrado = "fvelez";
    private const string Sal = "UISRAEL-TA1-fvelez";

    // SHA-256(Sal + "Uisrael2026*") en hexadecimal.
    private const string HashClave = "FDBD3C1F1D437159E889678FD5E2C54B6306D8ABF47E3FDB4C4230B2125BBF6D";

    private static int _intentosFallidos;
    private static DateTime? _bloqueadoHasta;

    public static bool EstaBloqueado(out TimeSpan restante)
    {
        restante = TimeSpan.Zero;
        if (_bloqueadoHasta is null) return false;

        restante = _bloqueadoHasta.Value - DateTime.Now;
        if (restante > TimeSpan.Zero) return true;

        _bloqueadoHasta = null;
        _intentosFallidos = 0;
        return false;
    }

    /// <summary>Devuelve true si el usuario y la contraseña son correctos.</summary>
    public static bool ValidarCredenciales(string? usuario, string? clave)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave)) return false;

        var usuarioCorrecto = string.Equals(usuario.Trim(), UsuarioRegistrado, StringComparison.OrdinalIgnoreCase);
        var claveCorrecta = CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(CalcularHash(clave)),
            Convert.FromHexString(HashClave));

        if (usuarioCorrecto && claveCorrecta)
        {
            _intentosFallidos = 0;
            return true;
        }

        _intentosFallidos++;
        if (_intentosFallidos >= IntentosMaximos) _bloqueadoHasta = DateTime.Now.Add(TiempoBloqueo);
        return false;
    }

    public static int IntentosRestantes => Math.Max(0, IntentosMaximos - _intentosFallidos);

    public static string CalcularHash(string clave) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Sal + clave)));
}

/// <summary>Usuario con sesión iniciada en la app.</summary>
public static class Sesion
{
    public static string? Usuario { get; set; }
    public static bool Iniciada => !string.IsNullOrEmpty(Usuario);
}
