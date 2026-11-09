using PintagAPP.Presentacion.Views;

namespace PintagAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("RegistroPage", typeof(RegistroPage));
            Routing.RegisterRoute("OfrecerPage", typeof(OfrecerPage));
            Routing.RegisterRoute("MisOfertasPage", typeof(MisOfertasPage));
            Routing.RegisterRoute("OfertaFormPage", typeof(OfertaFormPage));
            Routing.RegisterRoute("PanelAdminPage", typeof(PanelAdminPage));
            Routing.RegisterRoute("RevisarPerfilPage", typeof(RevisarPerfilPage));
            Routing.RegisterRoute("OfertasCategoriaPage", typeof(OfertasCategoriaPage));
            Routing.RegisterRoute("DetalleOfertaPage", typeof(DetalleOfertaPage));
            Routing.RegisterRoute("CalificarPage", typeof(CalificarPage));
            Routing.RegisterRoute("PorCalificarPage", typeof(PorCalificarPage));
            Routing.RegisterRoute("DenunciasPage", typeof(DenunciasPage));
            Routing.RegisterRoute("MiCuentaPage", typeof(MiCuentaPage));
            Routing.RegisterRoute("PoliticaPrivacidadPage", typeof(PoliticaPrivacidadPage));
            Routing.RegisterRoute("EstadisticasPage", typeof(EstadisticasPage));
            Routing.RegisterRoute("AdminNoticiasPage", typeof(AdminNoticiasPage));
            Routing.RegisterRoute("NoticiaFormPage", typeof(NoticiaFormPage));
        }
    }
}