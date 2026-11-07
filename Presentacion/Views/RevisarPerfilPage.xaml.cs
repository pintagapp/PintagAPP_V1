using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class RevisarPerfilPage : ContentPage
    {
        private readonly RevisarPerfilViewModel _viewModel;
        private bool _cargado;

        public RevisarPerfilPage(RevisarPerfilViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_cargado) return;   // carga una sola vez (al volver de un alerta no recarga)
            _cargado = true;
            await _viewModel.CargarAsync();
        }
    }
}