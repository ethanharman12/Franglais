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
        }
    }
}
