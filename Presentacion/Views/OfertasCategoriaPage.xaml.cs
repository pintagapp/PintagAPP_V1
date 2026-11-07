using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class OfertasCategoriaPage : ContentPage
    {
        private readonly OfertasCategoriaViewModel _viewModel;
        private bool _cargado;

        public OfertasCategoriaPage(OfertasCategoriaViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_cargado) return;   // al volver del detalle no recarga
            _cargado = true;
            await _viewModel.CargarAsync();
        }
    }
}