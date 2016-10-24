using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NoraGrace.Web.Startup))]
namespace NoraGrace.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
