using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class InicioPage : ContentPage
    {
        private readonly InicioViewModel _viewModel;

        public InicioPage(InicioViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        // Cada vez que la pantalla aparece, actualiza el saludo y carga las categorías
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();
        }
    }
}