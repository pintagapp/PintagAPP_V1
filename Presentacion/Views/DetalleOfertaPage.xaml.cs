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
            if (_cargado) return;   // al volver de WhatsApp o del login no recarga
            _cargado = true;
            await _viewModel.CargarAsync();
        }
    }
}