//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Routing;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Routing;
using Akka.Util.Internal;
using Akka.Bootstrap.Docker;
using System.Threading;

namespace Samples.Cluster.RoundRobin
{
    class Program
    {
        private static Config _clusterConfig;

        private static int backendNum = Environment.ProcessorCount;
        private static string hostName = "127.0.0.1";

        public static int totalRequest = 10;

        public static Stopwatch sw;

        static async Task Main(string[] args)
        {
            string value = Environment.GetEnvironmentVariable("TOTALREQUEST");
            if (value != null)
            {
                Console.WriteLine($"Get total request number from environment variable: TOTALREQUEST: {value}.");
                totalRequest = int.Parse(value);
            }

            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            _clusterConfig = section.AkkaConfig;

            switch (args[0].ToLower())
            {
                case "frontend":
                    // frontend
                    var client = GetFrontendWithRouter(new[] { "5054" });
                    await StartFrontend(client);
                    break;
                case "backend":
                    // backend
                    //await StartBackend(args);
                    GetBackendSystem(new string [0]);
                    break;
                case "router":
                    // router
                    var router = GetRouter(new[] { "2553" });
                    break;
                case "seed-node":
                    LaunchSeedNode(new[] { "2551" });
                    break;
                default:
                    Console.WriteLine("Only support frontend, backend, router, seed-node");
                    break;
            }
            await Task.Delay(-1);
        }

        static void LaunchSeedNode(string[] args)
        {
            var port = args.Length > 0 ? args[0] : "2551";
            var config =
                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [seed]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + hostName))
                        .WithFallback(_clusterConfig);
            var system = ActorSystem.Create("ClusterSystem", config);
        }

        static void LaunchBackend(string[] args)
        {
            Console.WriteLine($"core: {backendNum}");
            var port = args.Length > 0 ? args[0] : "0";
            var config =
                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [backend]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + hostName))
                        .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config.BootstrapFromDocker());
            //var system = ActorSystem.Create("ClusterSystem", config);
            var backend = system.ActorOf(Props.Create<BackendActor>(), "backend");
            Console.WriteLine($"Backend path: {backend.Path}");
        }

        static IActorRef GetFrontend(string[] args)
        {
            var port = args.Length > 0 ? args[0] : "0";
            var config =
                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [frontend]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + hostName))
                        .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config);

            //var backendRouter =
            //    system.ActorOf(
            //        Props.Empty.WithRouter(new ClusterRouterGroup(new RoundRobinGroup("/user/backend"),
            //            new ClusterRouterGroupSettings(10, ImmutableHashSet.Create("/user/backend"), false, "backend"))));

            var workers = new[] { "/user/backend" };
            var backendRouter =
                system.ActorOf(
                    Props.Empty.WithRouter(new ClusterRouterGroup(new RoundRobinGroup(workers),
                        new ClusterRouterGroupSettings(1000, workers, true, "backend"))));
            var frontend = system.ActorOf(Props.Create(() => new FrontendActor(backendRouter)), "frontend");

            return frontend;
        }

        static async Task StartBackend(string[] args)
        {
            int currentBackendNum = 0;

            if (args.Length >= 2)
            {
                backendNum = int.Parse(args[1]);
            }
            else
            {
                LaunchBackend(new[] { "2551" });
                LaunchBackend(new[] { "2552" });

                currentBackendNum = 2;
            }
            while (currentBackendNum < backendNum)
            {
                LaunchBackend(new string[0]);
                currentBackendNum++;
                Console.WriteLine($"Launch {currentBackendNum} backend actors.");
            }
        }

        static async Task StartFrontend(IActorRef client)
        {
            await Task.Delay(TimeSpan.FromSeconds(20));

            sw = Stopwatch.StartNew();

            for (int i = 0; i < totalRequest; i++)
            {
                client.Tell(new StartCommand("hello-" + i));
            }
        }

        static IActorRef GetRouter(string[] args)
        {
            var port = args.Length > 0 ? args[0] : "2553";
            var config =
                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [router]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + hostName))
                        .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config.BootstrapFromDocker());
            //var system = ActorSystem.Create("ClusterSystem", config);

            var workers = new[] { "/user/backend" };

            var backendRouter = system.ActorOf(Props.Create<BackendActor>().WithRouter(new RoundRobinPool(16)), "router");

            //var backendRouter = system.ActorOf(Props.Create<BackendActor>().WithRouter(
            //         new ClusterRouterPool(
            //             new RoundRobinPool(1000),
            //             new ClusterRouterPoolSettings(1000, 16, false, "backend"))), "router");
            
            Console.WriteLine($"Router path: {backendRouter.Path}");
            Console.WriteLine("Router start.");
            return backendRouter;
        }

        static IActorRef GetFrontendWithRouter(string[] args)
        {
            var port = args.Length > 0 ? args[0] : "0";
            var config =
                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [frontend]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + hostName))
                        .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config.BootstrapFromDocker());
            //var system = ActorSystem.Create("ClusterSystem", config);

            var frontend = system.ActorOf(Props.Create(() => new FrontendActor()), "frontend");
            Console.WriteLine($"Frontend path: {frontend.Path}");

            return frontend;
        }

        static void GetBackendSystem(string[] args)
        {
            var port = args.Length > 0 ? args[0] : "2551";
            var config =
                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [backend]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=" + hostName))
                        .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config.BootstrapFromDocker());
            //var system = ActorSystem.Create("ClusterSystem", config);
        }
    }
}

