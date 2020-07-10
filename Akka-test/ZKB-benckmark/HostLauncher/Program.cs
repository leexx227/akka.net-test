using Akka.Actor;
using Akka.Configuration;
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
            var defaultConfig = section.AkkaConfig;

            var config = 
                ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + Environment.MachineName)
                .WithFallback(defaultConfig);

            var system = ActorSystem.Create("ZKB", config);

            system.WhenTerminated.Wait();
        }
    }
}
