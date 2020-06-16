using Akka.Actor;
using Akka.Configuration.Hocon;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ZKB.HostLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var config = section.AkkaConfig;

            var system = ActorSystem.Create("ZKB", config);

            system.WhenTerminated.Wait();
        }
    }
}
