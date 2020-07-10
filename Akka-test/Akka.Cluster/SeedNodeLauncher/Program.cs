using Akka.Actor;
using Akka.Configuration.Hocon;
using System;
using System.Configuration;

namespace Akka.Cluster.SeedNodeLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var config = section.AkkaConfig;

            var system = ActorSystem.Create("ClusterSystem", config);

            system.WhenTerminated.Wait();
        }
    }
}
