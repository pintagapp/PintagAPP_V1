using PintagAPP.Presentacion.ViewModels;
namespace PintagAPP.Presentacion.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
