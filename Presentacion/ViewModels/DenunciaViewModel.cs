using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PintagAPP.CasosUso;
using PintagAPP.CasosUso.Denuncias;
using PintagAPP.Dominio.Entidades;

namespace PintagAPP.Presentacion.ViewModels
{
    public record DenunciaItem(Denuncia Denuncia)
    {
        public string TipoTexto => Denuncia.Tipo switch
        {
            "oferta" => "🛒 Oferta",
            "opinion" => "💬 Opinión",
            _ => "👤 Perfil"
        };
        public string Descripcion => Denuncia.DescripcionObjetivo;
        public string Motivo => "Motivo: " + Denuncia.Motivo;
        public string Fecha => Denuncia.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    public partial class DenunciasViewModel : ObservableObject
    {
        private const string AccionPausar = "⏸️ Pausar oferta";
        private const string AccionBloquear = "⛔ Bloquear perfil";
        private const string AccionOcultar = "🙈 Ocultar opinión";
        private const string AccionDescartar = "✔️ Descartar (no pasa nada)";

        private readonly GestionarDenuncias _gestionar;

        public DenunciasViewModel(GestionarDenuncias gestionar)
        {
            _gestionar = gestionar;
        }

        public ObservableCollection<DenunciaItem> Denuncias { get; } = new();

        [ObservableProperty] private DenunciaItem? denunciaSeleccionada;
        [ObservableProperty] private string aviso = "";
        [ObservableProperty] private bool estaCargando;

        public async Task CargarAsync()
        {
            EstaCargando = true;
            Denuncias.Clear();
            try
            {
                if (!_gestionar.EsAdmin)
                {
                    Aviso = "Esta sección es solo para el administrador.";
                    return;
                }

                var lista = await _gestionar.ObtenerPendientesAsync();
                foreach (var d in lista)
                    Denuncias.Add(new DenunciaItem(d));

                Aviso = Denuncias.Count == 0
                    ? "✅ No hay denuncias pendientes."
                    : $"Denuncias por revisar: {Denuncias.Count}. Toca una para atenderla.";
            }
            catch (Exception)
            {
                Aviso = "No se pudieron cargar las denuncias. Revisa tu conexión.";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private Task RecargarAsync() => CargarAsync();

        async partial void OnDenunciaSeleccionadaChanged(DenunciaItem? value)
        {
            if (value is null) return;
            DenunciaSeleccionada = null;
            await AtenderAsync(value.Denuncia);
        }

        // Las acciones posibles dependen de qué se denunció
        private async Task AtenderAsync(Denuncia d)
        {
            string[] opciones = d.Tipo switch
            {
                "oferta" => new[] { AccionPausar, AccionBloquear, AccionDescartar },
                "opinion" => new[] { AccionOcultar, AccionDescartar },
                _ => new[] { AccionBloquear, AccionDescartar }
            };

            var opcion = await Shell.Current.DisplayActionSheetAsync(
                "¿Qué hacemos con esta denuncia?", "Cancelar", null, opciones);

            Resultado? resultado = opcion switch
            {
                AccionPausar => await _gestionar.PausarOfertaAsync(d),
                AccionBloquear => await _gestionar.BloquearPerfilAsync(d),
                AccionOcultar => await _gestionar.OcultarOpinionAsync(d),
                AccionDescartar => await _gestionar.DescartarAsync(d),
                _ => null   // canceló
            };
            if (resultado is null) return;

            await Shell.Current.DisplayAlertAsync(resultado.EsExito ? "Listo" : "Error", resultado.Mensaje, "Aceptar");
            if (resultado.EsExito)
                await CargarAsync();
        }
    }
}