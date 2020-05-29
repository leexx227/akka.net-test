//-----------------------------------------------------------------------
// <copyright file="FrontendActor.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;

namespace Samples.Cluster.RoundRobin
{
    public class FrontendActor : UntypedActor, IWithUnboundedStash
    {
        protected readonly IActorRef BackendRouter;
        protected int jobCount = 0;
        protected int sendJobCount = 0;

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        private string routerPath = "akka.tcp://ClusterSystem@akka-test-router-0.akka-test-router:2553/user/router";

        public FrontendActor(IActorRef backendRouter)
        {
            BackendRouter = backendRouter;
        }

        public FrontendActor()
        {
            BackendRouter = default(IActorRef);
        }

        protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);

        /// <summary>
        /// Need to subscribe to cluster changes
        /// </summary>
        protected override void PreStart()
        {
            Cluster.Subscribe(Self, new[] { typeof(ClusterEvent.MemberUp) });
        }

        /// <summary>
        /// Re-subscribe on restart
        /// </summary>
        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
        }

        protected override void OnReceive(object message)
        {
            if (message is ClusterEvent.MemberUp)
            {
                Console.WriteLine("Frontend [{0}]: Cluster is ready. Able to begin jobs.");
                //ready to begin routing messages to back-end
                Become(ReadyToProcess);
                Stash.UnstashAll();
            }
            else
            {
                Stash.Stash();
            }
        }

        protected void ReadyToProcess(object message)
        {
            if (message is StartCommand)
            {
                sendJobCount++;
                var sc = message as StartCommand;
                if (BackendRouter == default(IActorRef))
                {
                    Context.ActorSelection(routerPath).Tell(new FrontendCommand()
                    {
                        Message = string.Format("message {0}", sendJobCount),
                        JobId = sc.CommandText
                    });
                }
                else
                {
                    BackendRouter.Tell(new FrontendCommand()
                    {
                        Message = string.Format("message {0}", sendJobCount),
                        JobId = sc.CommandText
                    });
                }

                //Console.WriteLine($"Frontend [{Cluster.SelfAddress}]: Send request: {sendJobCount}");
                Log.Info($"Frontend [{Cluster.SelfAddress}]: Send request: {sendJobCount}");
            }
            else if (message is CommandComplete)
            {
                jobCount++;
                //Console.WriteLine($"[{Program.sw.ElapsedMilliseconds}]Frontend [{Cluster.SelfAddress}]: Received {jobCount} CommandComplete from {Sender}");
                Log.Info($"[{Program.sw.ElapsedMilliseconds}]Frontend [{Cluster.SelfAddress}]: Received {jobCount} CommandComplete from {Sender}");
                if (jobCount == Program.totalRequest)
                {
                    Console.WriteLine($"[{Program.sw.ElapsedMilliseconds}]Frontend [{Cluster.SelfAddress}]: Finish jobs.");
                    Log.Info($"[{Program.sw.ElapsedMilliseconds}]Frontend [{Cluster.SelfAddress}]: Finish jobs.");
                }
            }
        }

        public IStash Stash { get; set; }
    }
}

