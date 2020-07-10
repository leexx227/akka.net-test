using Akka.Actor;
using Akka.Cluster.Actors;
using Akka.Cluster.Routing;
using Akka.Configuration.Hocon;
using Akka.Routing;
using System;
using System.Configuration;

namespace Akka.Cluster.RouterLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var config = section.AkkaConfig;

            var system = ActorSystem.Create("ClusterSystem", config);

            var backendRouter = system.ActorOf(Props.Create<BackendActor>().WithRouter(
                                 new ClusterRouterPool(
                                     new RoundRobinPool(1000),
                                     new ClusterRouterPoolSettings(1000, 10, true, "backend"))), "router");
            system.WhenTerminated.Wait();
        }
    }
}
