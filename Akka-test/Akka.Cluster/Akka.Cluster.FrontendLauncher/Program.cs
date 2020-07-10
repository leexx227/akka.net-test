using Akka.Actor;
using Akka.Cluster.Actors;
using Akka.Cluster.Messages;
using Akka.Cluster.Routing;
using Akka.Configuration.Hocon;
using Akka.Routing;
using System;
using System.Configuration;
using System.Linq;

namespace Akka.Cluster.FrontendLauncher
{
    class Program
    {
        public static string routerAddress = "akka.tcp://ClusterSystem@AkkaDotNetRoute:2553/user/router";
        static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var config = section.AkkaConfig;

            var system = ActorSystem.Create("ClusterSystem", config);

            var frontend = system.ActorOf(Props.Create(() => new FrontendActor(routerAddress)), "frontend");

            system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), ()=>
            {
                frontend.Tell(new StartMessage());
            });

            system.WhenTerminated.Wait();
        }
    }
}
