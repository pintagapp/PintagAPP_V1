using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class CalificarPage : ContentPage
    {
        private readonly CalificarViewModel _viewModel;
        private bool _cargado;

        public CalificarPage(CalificarViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_cargado) return;
            _cargado = true;
            await _viewModel.CargarAsync();
        }
    }
}