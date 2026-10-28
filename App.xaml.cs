using PintagAPP.CasosUso.Cuenta;

namespace PintagAPP
{
    public partial class App : Application
    {
        private readonly ControlSesion _controlSesion;

        public App(ControlSesion controlSesion)
        {
            InitializeComponent();
            _controlSesion = controlSesion;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await _controlSesion.RestaurarAsync();   // si ya había iniciado sesión antes
        }
    }
}