using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class AdminNoticiasPage : ContentPage
    {
        private readonly AdminNoticiasViewModel _viewModel;

        public AdminNoticiasPage(AdminNoticiasViewModel viewModel)
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