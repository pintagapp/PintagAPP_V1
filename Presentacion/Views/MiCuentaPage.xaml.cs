using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class MiCuentaPage : ContentPage
    {
        public MiCuentaPage(MiCuentaViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}