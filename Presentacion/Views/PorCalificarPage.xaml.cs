using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class PorCalificarPage : ContentPage
    {
        private readonly PorCalificarViewModel _viewModel;

        public PorCalificarPage(PorCalificarViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();
        }
    }
}