using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JobSeekersFinal.Startup))]
namespace JobSeekersFinal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
