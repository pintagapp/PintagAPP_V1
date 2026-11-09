using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class DenunciasPage : ContentPage
    {
        private readonly DenunciasViewModel _viewModel;

        public DenunciasPage(DenunciasViewModel viewModel)
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