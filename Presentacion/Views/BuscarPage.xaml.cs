using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class BuscarPage : ContentPage
    {
        private readonly BuscarViewModel _viewModel;

        public BuscarPage(BuscarViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        // Al entrar a la pestaña sin texto, el cursor ya queda en el buscador
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (string.IsNullOrEmpty(_viewModel.Texto))
                buscador.Focus();
        }
    }
}