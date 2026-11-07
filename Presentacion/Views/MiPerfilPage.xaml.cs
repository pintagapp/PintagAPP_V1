using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class MiPerfilPage : ContentPage
    {
        private readonly MiPerfilViewModel _viewModel;

        public MiPerfilPage(MiPerfilViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.ActualizarAsync();   // refleja si se inició o cerró sesión y los pendientes del admin
        }
    }
}