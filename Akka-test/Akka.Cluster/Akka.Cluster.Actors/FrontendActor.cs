using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Messages;
using Akka.Event;
using Akka.Routing;
using System;
using System.Linq;

namespace Akka.Cluster.Actors
{
    public class FrontendActor : ReceiveActor, IWithUnboundedStash
    {
        private string routerAddress;

        private static string defaultRouterAddress = "akka.tcp://ClusterSystem@127.0.0.1:2553/user/router";

        protected Cluster Cluster = Cluster.Get(Context.System);

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        private int sendRequestCount = 0;

        private int receiveResponseCount = 0;

        public FrontendActor() : this(defaultRouterAddress)
        { 

        }

        public FrontendActor(string routerAddress)
        {
            this.routerAddress = routerAddress;

            Waiting();
        }

        protected override void PreStart()
        {
            Cluster.Subscribe(Self, new[] { typeof(ClusterEvent.MemberUp) });
        }

        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
        }

        private void Waiting()
        {
            Receive<ClusterEvent.MemberUp>(message =>
            {
                Log.Info($"[Frontend: {Context.Self}] Cluster is ready. Able to begin jobs.");
                Become(Sending);
                Stash.UnstashAll();
            });
            ReceiveAny(_ => Stash.Stash());
        }

        private void Sending()
        {
            Receive<StartMessage>(_ =>
            {
                if (Context.ActorSelection(this.routerAddress).Ask<Routees>(new GetRoutees()).Result.Members.Any())
                {
                    sendRequestCount++;
                    Context.ActorSelection(this.routerAddress).Tell(new RequestMessage(sendRequestCount));
                    Log.Info($"[{Context.Self}] send {sendRequestCount} request.");
                }
            });
            Receive<RequestCompleteMessage>(_ =>
            {
                receiveResponseCount++;
                Log.Info($"[Frontend: {Self}] get {receiveResponseCount} response from {Sender}.");
            });
        }

        public IStash Stash { get; set; }
    }
}
