using System.Text;
using System.Text.RegularExpressions;

namespace fvelezTA1.Services;

/// <summary>
/// Reglas de validación del formulario. Se usan de dos formas:
/// 1) Filtrar*: limpian lo que el usuario escribe en tiempo real (solo números / solo letras).
/// 2) Es*: comprueban el valor completo al presionar «Enviar Inscripción».
/// </summary>
public static partial class Validaciones
{
    public const string PatronLetras = @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü]+( [A-Za-zÁÉÍÓÚáéíóúÑñÜü]+)*$";
    public const string PatronCelular = @"^09[0-9]{8}$";

    [GeneratedRegex(PatronLetras)]
    private static partial Regex RegexLetras();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$")]
    private static partial Regex RegexCorreo();

    [GeneratedRegex(@"^[A-Z0-9]{5,15}$")]
    private static partial Regex RegexPasaporte();

    [GeneratedRegex(PatronCelular)]
    private static partial Regex RegexCelular();

    // ---------- Filtros en tiempo real ----------

    /// <summary>Deja solo los dígitos 0-9.</summary>
    public static string FiltrarNumeros(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto)
            if (char.IsAsciiDigit(c)) sb.Append(c);
        return sb.ToString();
    }

    /// <summary>Deja solo letras (con tildes y ñ) y un espacio simple entre palabras.</summary>
    public static string FiltrarLetras(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto)
        {
            if (char.IsLetter(c)) sb.Append(c);
            else if (c == ' ' && sb.Length > 0 && sb[^1] != ' ') sb.Append(' ');
        }
        return sb.ToString();
    }

    /// <summary>Deja letras A-Z y números, en mayúsculas (pasaporte).</summary>
    public static string FiltrarAlfanumerico(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto.ToUpperInvariant())
            if (char.IsAsciiLetterUpper(c) || char.IsAsciiDigit(c)) sb.Append(c);
        return sb.ToString();
    }

    /// <summary>Quita espacios del correo mientras se escribe.</summary>
    public static string FiltrarCorreo(string? texto) =>
        string.IsNullOrEmpty(texto) ? string.Empty : texto.Replace(" ", string.Empty);

    // ---------- Validaciones completas ----------

    public static bool EsSoloLetras(string? texto, int minimo = 2) =>
        !string.IsNullOrWhiteSpace(texto) && texto.Trim().Length >= minimo && RegexLetras().IsMatch(texto.Trim());

    public static bool EsCorreoValido(string? correo) =>
        !string.IsNullOrWhiteSpace(correo) && RegexCorreo().IsMatch(correo.Trim());

    public static bool CorreosCoinciden(string? correo, string? confirmacion) =>
        !string.IsNullOrWhiteSpace(correo) &&
        string.Equals(correo.Trim(), confirmacion?.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>Celular de Ecuador: 10 dígitos y empieza con 09.</summary>
    public static bool EsCelularValido(string? telefono) =>
        !string.IsNullOrEmpty(telefono) && RegexCelular().IsMatch(telefono);

    public static bool EsPasaporteValido(string? pasaporte) =>
        !string.IsNullOrEmpty(pasaporte) && RegexPasaporte().IsMatch(pasaporte);

    /// <summary>
    /// Cédula ecuatoriana: 10 dígitos, provincia 01-24 (o 30), tercer dígito menor a 6
    /// y dígito verificador según el algoritmo módulo 10.
    /// </summary>
    public static bool EsCedulaValida(string? cedula)
    {
        if (cedula is null || cedula.Length != 10 || !cedula.All(char.IsAsciiDigit)) return false;
        if (!EsProvinciaValida(cedula)) return false;
        if (cedula[2] - '0' >= 6) return false;

        var suma = 0;
        for (var i = 0; i < 9; i++)
        {
            var producto = (cedula[i] - '0') * (i % 2 == 0 ? 2 : 1);
            suma += producto > 9 ? producto - 9 : producto;
        }

        var verificador = (10 - suma % 10) % 10;
        return verificador == cedula[9] - '0';
    }

    /// <summary>
    /// RUC: 13 dígitos que terminan en 001 y provincia válida.
    /// Para persona natural (tercer dígito menor a 6) los 10 primeros deben ser una cédula válida.
    /// </summary>
    public static bool EsRucValido(string? ruc)
    {
        if (ruc is null || ruc.Length != 13 || !ruc.All(char.IsAsciiDigit)) return false;
        if (!ruc.EndsWith("001", StringComparison.Ordinal) || !EsProvinciaValida(ruc)) return false;
        var tercero = ruc[2] - '0';
        if (tercero < 6) return EsCedulaValida(ruc[..10]);
        return tercero == 6 || tercero == 9; // entidad pública o sociedad privada
    }

    /// <summary>Valida el número según el tipo de identificación elegido.</summary>
    public static string? ValidarIdentificacion(string tipo, string? numero) => tipo switch
    {
        Catalogos.Cedula when string.IsNullOrEmpty(numero) => "Ingresa el número de cédula.",
        Catalogos.Cedula when !EsCedulaValida(numero) => "La cédula no es válida. Revisa los 10 dígitos.",
        Catalogos.Ruc when string.IsNullOrEmpty(numero) => "Ingresa el número de RUC.",
        Catalogos.Ruc when !EsRucValido(numero) => "El RUC debe tener 13 dígitos y terminar en 001.",
        Catalogos.Pasaporte when string.IsNullOrEmpty(numero) => "Ingresa el número de pasaporte.",
        Catalogos.Pasaporte when !EsPasaporteValido(numero) => "El pasaporte debe tener de 5 a 15 letras o números.",
        _ => null
    };

    private static bool EsProvinciaValida(string numero)
    {
        var provincia = (numero[0] - '0') * 10 + (numero[1] - '0');
        return provincia is >= 1 and <= 24 or 30;
    }
}
