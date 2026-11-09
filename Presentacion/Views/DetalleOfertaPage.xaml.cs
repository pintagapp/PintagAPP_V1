using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class DetalleOfertaPage : ContentPage
    {
        private readonly DetalleOfertaViewModel _viewModel;
        private bool _cargado;

        public DetalleOfertaPage(DetalleOfertaViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_cargado)
            {
                _cargado = true;
                await _viewModel.CargarAsync();
            }
            else
            {
                // Volvió de calificar o de iniciar sesión
                await _viewModel.RefrescarAsync();
                await _viewModel.AlVolverALaAppAsync();
            }
        }

        // Para el recordatorio: detectar cuando la app vuelve de WhatsApp o del marcador
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            if (Window is not null)
                Window.Resumed += AlReanudarApp;
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);
            if (Window is not null)
                Window.Resumed -= AlReanudarApp;
        }

        private async void AlReanudarApp(object? sender, EventArgs e)
        {
            await _viewModel.AlVolverALaAppAsync();
        }
    }
}