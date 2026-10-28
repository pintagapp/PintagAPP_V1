using PintagAPP.Presentacion.ViewModels;

namespace PintagAPP.Presentacion.Views
{
    public partial class RegistroPage : ContentPage
    {
        public RegistroPage(RegistroViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}