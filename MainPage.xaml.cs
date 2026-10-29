using fvelezTA1.Models;
using fvelezTA1.Services;

namespace fvelezTA1;

/// <summary>
/// Taller Semana 1 · Formulario de inscripción (réplica del formulario Fase A - Grado del SIGE UISRAEL).
/// Validaciones: 1) solo números, 2) solo letras, 3) coincidencia de correos, 4) envío a archivo TXT.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>Ancho a partir del cual los campos en pareja se muestran lado a lado.</summary>
    private const double AnchoDosColumnas = 600;

    private static readonly Color ColorBorde = Color.FromArgb("#C9CED8");
    private static readonly Color ColorBordeError = Color.FromArgb("#C62828");

    private readonly Grid[] _pares;
    private readonly Dictionary<View, Label> _errores;
    private bool? _dosColumnas;

    public MainPage()
    {
        InitializeComponent();

        _pares = [parIdentificacion, parApellidos, parCorreos, parModalidadCampus];

        _errores = new()
        {
            [txtNumeroId] = lblErrorId,
            [txtPaterno] = lblErrorPaterno,
            [txtMaterno] = lblErrorMaterno,
            [txtNombres] = lblErrorNombres,
            [txtTelefono] = lblErrorTelefono,
            [txtCorreo] = lblErrorCorreo,
            [txtConfirmarCorreo] = lblErrorConfirmar,
            [pkCarrera] = lblErrorCarrera,
            [pkModalidad] = lblErrorModalidad,
            [pkCampus] = lblErrorCampus
        };

        // Se cargan después de _errores porque cambiar SelectedIndex dispara eventos que lo usan.
        pkTipoId.ItemsSource = Catalogos.TiposIdentificacion;
        pkTipoId.SelectedIndex = 0;
        pkCarrera.ItemsSource = Catalogos.Carreras;
        pkModalidad.ItemsSource = Catalogos.Modalidades;
        pkCampus.ItemsSource = Catalogos.Campus;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Protección de datos: sin sesión iniciada no se puede usar el formulario.
        if (!Sesion.Iniciada)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        lblSesion.Text = $"Sesión iniciada: {Sesion.Usuario} · Los datos se guardan solo en este dispositivo.";
    }

    // =====================================================================
    // INTERFAZ ADAPTABLE
    // =====================================================================

    /// <summary>
    /// Pantalla ancha (≥ 600 px, Windows o tablet): los campos en pareja van lado a lado.
    /// Celular: todos los campos en una sola columna.
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0) return;

        var dosColumnas = width >= AnchoDosColumnas;
        if (_dosColumnas == dosColumnas) return;   // solo se reorganiza al cruzar el punto de quiebre
        _dosColumnas = dosColumnas;

        contenedor.Padding = dosColumnas ? new Thickness(40, 24) : new Thickness(20, 16);

        foreach (var par in _pares)
        {
            var primero = (View)par.Children[0];
            var segundo = (View)par.Children[1];
            par.ColumnDefinitions.Clear();
            par.RowDefinitions.Clear();

            if (dosColumnas)
            {
                par.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                par.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                par.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(segundo, 0);
                Grid.SetColumn(segundo, 1);
            }
            else
            {
                par.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                par.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                par.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(segundo, 1);
                Grid.SetColumn(segundo, 0);
            }

            Grid.SetRow(primero, 0);
            Grid.SetColumn(primero, 0);
        }
    }

    // =====================================================================
    // VALIDACIÓN EN TIEMPO REAL
    // =====================================================================

    /// <summary>Validación 2 · Solo letras: elimina números y símbolos mientras se escribe.</summary>
    private void OnSoloLetrasChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry) AplicarFiltro(entry, e.NewTextValue, Validaciones.FiltrarLetras);
    }

    /// <summary>Validación 1 · Solo números: elimina letras y símbolos mientras se escribe.</summary>
    private void OnSoloNumerosChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry) AplicarFiltro(entry, e.NewTextValue, Validaciones.FiltrarNumeros);
    }

    /// <summary>Cédula y RUC: solo números. Pasaporte: letras y números en mayúsculas.</summary>
    private void OnNumeroIdentificacionChanged(object? sender, TextChangedEventArgs e)
    {
        Func<string?, string> filtro = TipoSeleccionado == Catalogos.Pasaporte
            ? Validaciones.FiltrarAlfanumerico
            : Validaciones.FiltrarNumeros;
        AplicarFiltro(txtNumeroId, e.NewTextValue, filtro);
    }

    private void OnTipoIdentificacionChanged(object? sender, EventArgs e)
    {
        (txtNumeroId.MaxLength, txtNumeroId.Keyboard, txtNumeroId.Placeholder) = TipoSeleccionado switch
        {
            Catalogos.Ruc => (13, Keyboard.Numeric, "13 dígitos"),
            Catalogos.Pasaporte => (15, Keyboard.Text, "Letras y números"),
            _ => (10, Keyboard.Numeric, "10 dígitos")
        };
        txtNumeroId.Text = string.Empty;
        lblErrorId.IsVisible = false;
        ResaltarCaja(txtNumeroId, false);
    }

    /// <summary>Validación 3 · Avisa en vivo si el correo y su confirmación coinciden.</summary>
    private void OnCorreoChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry) AplicarFiltro(entry, e.NewTextValue, Validaciones.FiltrarCorreo);

        if (string.IsNullOrEmpty(txtConfirmarCorreo.Text))
        {
            lblErrorConfirmar.IsVisible = false;
            return;
        }

        var coinciden = Validaciones.CorreosCoinciden(txtCorreo.Text, txtConfirmarCorreo.Text);
        lblErrorConfirmar.Text = coinciden ? "Los correos coinciden." : "Los correos no coinciden.";
        lblErrorConfirmar.TextColor = coinciden ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828");
        lblErrorConfirmar.IsVisible = true;
    }

    private void OnSelectorChanged(object? sender, EventArgs e)
    {
        if (sender is View vista && _errores.TryGetValue(vista, out var error))
        {
            error.IsVisible = false;
            ResaltarCaja(vista, false);
        }
    }

    private void AplicarFiltro(Entry entry, string? nuevo, Func<string?, string> filtro)
    {
        var limpio = filtro(nuevo);
        if (limpio != (nuevo ?? string.Empty))
        {
            entry.Text = limpio;
            entry.CursorPosition = limpio.Length;
        }

        if (entry != txtConfirmarCorreo && _errores.TryGetValue(entry, out var error))
        {
            error.IsVisible = false;
            ResaltarCaja(entry, false);
        }
    }

    /// <summary>Pinta de rojo (o devuelve a gris) el borde de la caja que contiene al campo.</summary>
    private static void ResaltarCaja(View campo, bool conError)
    {
        Element? actual = campo.Parent;
        while (actual is not null and not Border) actual = actual.Parent;
        if (actual is Border caja) caja.Stroke = new SolidColorBrush(conError ? ColorBordeError : ColorBorde);
    }

    private string TipoSeleccionado => pkTipoId.SelectedItem as string ?? Catalogos.Cedula;

    // =====================================================================
    // GUARDAR DATOS
    // =====================================================================

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        var errores = ValidarFormulario(out var primerCampoInvalido);
        if (errores.Count > 0)
        {
            await DisplayAlert("Error",
                $"Hay {errores.Count} dato(s) por corregir:\n\n• {string.Join("\n• ", errores)}", "OK");
            primerCampoInvalido?.Focus();
            return;
        }

        var inscripcion = new Inscripcion
        {
            TipoIdentificacion = TipoSeleccionado,
            NumeroIdentificacion = txtNumeroId.Text!.Trim(),
            ApellidoPaterno = txtPaterno.Text!.Trim().ToUpperInvariant(),
            ApellidoMaterno = txtMaterno.Text!.Trim().ToUpperInvariant(),
            Nombres = txtNombres.Text!.Trim().ToUpperInvariant(),
            Telefono = txtTelefono.Text!.Trim(),
            Correo = txtCorreo.Text!.Trim().ToLowerInvariant(),
            Carrera = (string)pkCarrera.SelectedItem,
            Modalidad = (string)pkModalidad.SelectedItem,
            Campus = (string)pkCampus.SelectedItem
        };

        try
        {
            btnGuardar.IsEnabled = false;

            // Validación 4 · Enviar todo a un archivo TXT
            var ruta = await RegistroArchivo.GuardarAsync(inscripcion, Sesion.Usuario);

            await DisplayAlert("Resultado",
                $"Bienvenido \"{inscripcion.Nombres} {inscripcion.ApellidoPaterno}\"\n" +
                $"Carrera: \"{inscripcion.Carrera}\" · {inscripcion.Modalidad}\n\n" +
                $"Datos guardados en:\n{ruta}", "OK");

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar el archivo TXT.\n{ex.Message}", "OK");
        }
        finally
        {
            btnGuardar.IsEnabled = true;
        }
    }

    /// <summary>Valida todos los campos, muestra el mensaje debajo de cada uno y devuelve la lista de errores.</summary>
    private List<string> ValidarFormulario(out View? primerCampoInvalido)
    {
        var errores = new List<string>();
        View? primero = null;

        void Marcar(View campo, string mensaje)
        {
            var etiqueta = _errores[campo];
            etiqueta.Text = mensaje;
            etiqueta.TextColor = Color.FromArgb("#C62828");
            etiqueta.IsVisible = true;
            ResaltarCaja(campo, true);
            errores.Add(mensaje);
            primero ??= campo;
        }

        foreach (var (campo, etiqueta) in _errores)
        {
            etiqueta.IsVisible = false;
            ResaltarCaja(campo, false);
        }

        // Identificación según el tipo (cédula con dígito verificador, RUC, pasaporte)
        var errorId = Validaciones.ValidarIdentificacion(TipoSeleccionado, txtNumeroId.Text?.Trim());
        if (errorId is not null) Marcar(txtNumeroId, errorId);

        // 2. Solo letras
        if (!Validaciones.EsSoloLetras(txtPaterno.Text))
            Marcar(txtPaterno, "Apellido paterno: obligatorio, solo letras (mínimo 2).");
        if (!Validaciones.EsSoloLetras(txtMaterno.Text))
            Marcar(txtMaterno, "Apellido materno: obligatorio, solo letras (mínimo 2).");
        if (!Validaciones.EsSoloLetras(txtNombres.Text))
            Marcar(txtNombres, "Nombres: obligatorios, solo letras (mínimo 2).");

        // 1. Solo números
        if (!Validaciones.EsCelularValido(txtTelefono.Text))
            Marcar(txtTelefono, "Teléfono: 10 dígitos que empiecen con 09.");

        // 3. Correos válidos y coincidentes
        if (!Validaciones.EsCorreoValido(txtCorreo.Text))
            Marcar(txtCorreo, "Correo: ingresa un correo válido, por ejemplo nombre@dominio.com.");
        if (string.IsNullOrWhiteSpace(txtConfirmarCorreo.Text))
            Marcar(txtConfirmarCorreo, "Confirma tu correo.");
        else if (!Validaciones.CorreosCoinciden(txtCorreo.Text, txtConfirmarCorreo.Text))
            Marcar(txtConfirmarCorreo, "Los correos no coinciden.");

        // Selectores obligatorios
        if (pkCarrera.SelectedIndex < 0) Marcar(pkCarrera, "Selecciona una carrera.");
        if (pkModalidad.SelectedIndex < 0) Marcar(pkModalidad, "Selecciona una modalidad.");
        if (pkCampus.SelectedIndex < 0) Marcar(pkCampus, "Selecciona un campus.");

        primerCampoInvalido = primero;
        return errores;
    }

    private void OnLimpiarClicked(object? sender, EventArgs e) => LimpiarFormulario();

    private void LimpiarFormulario()
    {
        pkTipoId.SelectedIndex = 0;
        foreach (var entry in new[] { txtNumeroId, txtPaterno, txtMaterno, txtNombres, txtTelefono, txtCorreo, txtConfirmarCorreo })
            entry.Text = string.Empty;
        pkCarrera.SelectedIndex = -1;
        pkModalidad.SelectedIndex = -1;
        pkCampus.SelectedIndex = -1;
        foreach (var (campo, etiqueta) in _errores)
        {
            etiqueta.IsVisible = false;
            ResaltarCaja(campo, false);
        }
    }

    // =====================================================================
    // ARCHIVO TXT Y SESIÓN
    // =====================================================================

    private async void OnVerArchivoClicked(object? sender, EventArgs e)
    {
        var contenido = await RegistroArchivo.LeerAsync();
        if (string.IsNullOrEmpty(contenido))
        {
            await DisplayAlert("Archivo TXT", "Todavía no hay inscripciones registradas.", "OK");
            return;
        }

        var total = RegistroArchivo.ContarRegistros(contenido);
        var vista = contenido.Length > 1500 ? "…\n" + contenido[^1500..] : contenido;
        var compartir = await DisplayAlert($"{RegistroArchivo.NombreArchivo} · {total} registro(s)",
            vista, "Compartir archivo", "Cerrar");

        if (compartir)
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Inscripciones UISRAEL",
                File = new ShareFile(RegistroArchivo.Ruta)
            });
        }
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("Cerrar sesión", "¿Deseas salir del formulario?", "Sí", "No")) return;

        LimpiarFormulario();
        Sesion.Usuario = null;
        await Shell.Current.GoToAsync("//login");
    }
}
