using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Franglais.Startup))]
namespace Franglais
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            GlobalHost.DependencyResolver.Register(
            typeof(ChatHub),
            () => new ChatHub(new GoogleTranslator()));

            app.MapSignalR();
        }
    }
}
