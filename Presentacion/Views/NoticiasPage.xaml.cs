using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class NoticiasPage : ContentPage
    {
        private readonly NoticiasViewModel _viewModel;

        public NoticiasPage(NoticiasViewModel viewModel)
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