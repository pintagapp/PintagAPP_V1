using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class PanelAdminPage : ContentPage
    {
        private readonly PanelAdminViewModel _viewModel;

        public PanelAdminPage(PanelAdminViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();   // al volver de aprobar/bloquear, la lista se actualiza
        }
    }
}