using fvelezTA1.Services;

namespace fvelezTA1;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        txtClave.Text = string.Empty;
        lblError.IsVisible = false;
    }

    private void OnCampoChanged(object? sender, TextChangedEventArgs e) => lblError.IsVisible = false;

    private void OnMostrarClaveClicked(object? sender, EventArgs e)
    {
        txtClave.IsPassword = !txtClave.IsPassword;
        btnMostrar.Text = txtClave.IsPassword ? "Ver" : "Ocultar";
    }

    private async void OnIngresarClicked(object? sender, EventArgs e)
    {
        if (Seguridad.EstaBloqueado(out var restante))
        {
            MostrarError($"Demasiados intentos. Espera {Math.Ceiling(restante.TotalSeconds)} segundos.");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrEmpty(txtClave.Text))
        {
            MostrarError("Ingresa tu usuario y tu contraseña.");
            return;
        }

        if (!Seguridad.ValidarCredenciales(txtUsuario.Text, txtClave.Text))
        {
            txtClave.Text = string.Empty;
            MostrarError(Seguridad.EstaBloqueado(out _)
                ? $"Usuario o contraseña incorrectos. Ingreso bloqueado por {Seguridad.TiempoBloqueo.TotalSeconds} segundos."
                : $"Usuario o contraseña incorrectos. Te quedan {Seguridad.IntentosRestantes} intento(s).");
            return;
        }

        Sesion.Usuario = txtUsuario.Text.Trim().ToLowerInvariant();
        txtClave.Text = string.Empty;
        await Shell.Current.GoToAsync("//inscripcion");
    }

    private void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
        lblError.IsVisible = true;
    }
}
