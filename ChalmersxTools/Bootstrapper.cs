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
            container.RegisterInstance<IUnityContainer>(container);
            container.RegisterType<LearningToolServerDbContext>();
            container.RegisterType<ISessionManager, SessionManager>();
            container.RegisterType<ITool, PresentationTool>(PresentationTool.CONSUMER_KEY);
            container.RegisterType<ITool, EarthSpheresImageTool>(EarthSpheresImageTool.CONSUMER_KEY);
            container.RegisterType<ITool, EarthMassTool>(EarthMassTool.CONSUMER_KEY);
        }
    }
}