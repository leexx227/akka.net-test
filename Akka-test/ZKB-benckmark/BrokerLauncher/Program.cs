﻿using Akka.Actor;
using Akka.Configuration.Hocon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ZKB.Actors;
using ZKB.Messages;

namespace ZKB.BrokerLauncher
{
    class Program
    {
        public static int totalRequestCount = 10;

        public static int messageLength = 20;

        public static int hostActorPerVM = 8;

        public static int requestTimeMilisec = 600;

        public static TaskCompletionSource<bool> ts = new TaskCompletionSource<bool>();

        public static Stopwatch sw;

        public static List<string> vMAddressList = new List<string>();

        static async Task Main(string[] args)
        {
            totalRequestCount = int.Parse(args[0]);
            messageLength = int.Parse(args[1]);

            vMAddressList.Add("akka.tcp://ZKB@127.0.0.1:2552");

            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var config = section.AkkaConfig;

            var system = ActorSystem.Create("ZKB", config);

            var requestQueueActor = system.ActorOf(Props.Create(() => new RequestQueueActor(totalRequestCount, messageLength)), "requestQueue");

            var dispatcherActor = system.ActorOf(Props.Create(() => new DispatcherActor(hostActorPerVM, requestTimeMilisec, requestQueueActor, totalRequestCount, ts, vMAddressList)), "dispatcher");

            sw = Stopwatch.StartNew();
            dispatcherActor.Tell(new StartMessage());
            await ts.Task;
            sw.Stop();
            Console.WriteLine($"Job finish in {sw.Elapsed.TotalSeconds} sec.");

            system.WhenTerminated.Wait();
        }
    }
}
