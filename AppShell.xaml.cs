using PintagAPP.Presentacion.Views;

namespace PintagAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("RegistroPage", typeof(RegistroPage));
        }
    }
}