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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Actualizar();   // refleja si se inició o cerró sesión
        }
    }
}