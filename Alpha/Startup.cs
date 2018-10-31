using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Alpha.Startup))]
namespace Alpha
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
