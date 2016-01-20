using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc4;
using ChalmersxTools.Sessions;
using ChalmersxTools.Database;
using ChalmersxTools.Tools;
using System.Configuration;
using System;

namespace ChalmersxTools
{
    public static class Bootstrapper
    {
        public static IUnityContainer Initialise()
        {
            var container = BuildUnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            return container;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            // Ugly fix for using local database when connection string is missing.
            var connectionString = ConfigurationManager.ConnectionStrings["chalmersxToolsConnectionString"];
            if (connectionString != null && connectionString.ToString() != "")
            {
                container.RegisterType<LearningToolServerDbContext>(new InjectionConstructor(true));
            }
            else
            {
                container.RegisterType<LearningToolServerDbContext>();
            }

            container.RegisterInstance<IUnityContainer>(container);
            container.RegisterType<ISessionManager, SessionManager>();
            container.RegisterType<ITool, PresentationTool>(PresentationTool.CONSUMER_KEY);
            container.RegisterType<ITool, EarthSpheresImageTool>(EarthSpheresImageTool.CONSUMER_KEY);
        }
    }
}