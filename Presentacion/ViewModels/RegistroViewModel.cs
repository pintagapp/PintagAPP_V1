using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso.Cuenta;

namespace PintagAPP.Presentacion.ViewModels
{
    public partial class RegistroViewModel : ObservableObject
    {
        private readonly RegistrarUsuario _registrarUsuario;

        public RegistroViewModel(RegistrarUsuario registrarUsuario)
        {
            _registrarUsuario = registrarUsuario;
        }

        [ObservableProperty] private string nombresCompletos = "";
        [ObservableProperty] private string cedula = "";
        [ObservableProperty] private string correo = "";
        [ObservableProperty] private string contrasena = "";
        [ObservableProperty] private bool aceptaAviso;
        [ObservableProperty] private bool estaOcupado;
        [ObservableProperty] private string mensaje = "";
        [ObservableProperty] private bool mensajeEsError;

        [RelayCommand]
        private async Task CrearCuentaAsync()
        {
            if (EstaOcupado) return;
            EstaOcupado = true;
            Mensaje = "";

            var resultado = await _registrarUsuario.EjecutarAsync(
                NombresCompletos, Cedula, Correo, Contrasena, AceptaAviso);

            EstaOcupado = false;

            if (!resultado.EsExito)
            {
                MensajeEsError = true;
                Mensaje = resultado.Mensaje;
                return;
            }

            // Éxito: avisa y regresa a la pantalla de iniciar sesión
            await Shell.Current.DisplayAlertAsync("¡Cuenta creada!",
                "Te enviamos un correo. Abre el enlace para confirmar tu cuenta y luego inicia sesión.",
                "Ir a iniciar sesión");

            NombresCompletos = Cedula = Correo = Contrasena = "";
            AceptaAviso = false;
            await Shell.Current.GoToAsync("..");
        }
    }
}