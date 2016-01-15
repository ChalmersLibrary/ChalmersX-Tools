using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ChalmersxTools.EventHandlers.OwinStartup))]

namespace ChalmersxTools.EventHandlers
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            Bootstrapper.Initialise();
        }
    }
}
