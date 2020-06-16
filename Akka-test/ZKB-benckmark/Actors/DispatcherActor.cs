using Akka.Actor;
using ZKB.Messages;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Akka.Event;

namespace ZKB.Actors
{
    public class DispatcherActor : ReceiveActor, IWithUnboundedStash
    {
        private List<string> vMNodeList = new List<string>();

        private int hostActorCountPerVM;

        private Queue<IActorRef> hostActorQueue = new Queue<IActorRef>();

        private int hostActorCount;

        private int hostActorReadyCount = 0;

        private int requestTimeMilisec;

        private int responseCount = 0;

        public IActorRef RequestQueueActorRef;

        private int totalRequestCount;

        private TaskCompletionSource<bool> ts;

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        public DispatcherActor(int hostActorCountPerVM, int requestTimeMilisec, IActorRef requestQueueActorRef, int totalRequestCount, TaskCompletionSource<bool> ts)
        {
            var vm = "akka.tcp://ZKB@127.0.0.1:2552";
            vMNodeList.Add(vm);

            this.hostActorCountPerVM = hostActorCountPerVM;
            this.hostActorCount = this.hostActorCountPerVM * vMNodeList.Count;
            this.requestTimeMilisec = requestTimeMilisec;
            this.RequestQueueActorRef = requestQueueActorRef;
            this.totalRequestCount = totalRequestCount;
            this.ts = ts;

            DeployHostActors();
        }

        private void DeployHostActors()
        {
            foreach(string vm in this.vMNodeList)
            {
                var remoteAddress = Address.Parse(vm);
                for (var i = 0; i < this.hostActorCountPerVM; i++)
                {
                    var hostActor = Context.ActorOf(
                        Props.Create(() => new HostActor(this.requestTimeMilisec))
                        .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))));
                    hostActorQueue.Enqueue(hostActor);
                }
            }
            Log.Debug($"[DispatcherActor] Finish deploying {hostActorQueue.Count} HostActors.");
            
            Receive<HostReadyMessage>(_ =>
            {
                Log.Debug("[DispatcherActor] receive HostReadyMessage.");
                hostActorReadyCount++;
                
                if (hostActorReadyCount == hostActorCount)
                {
                    //Self.Tell(new StartMessage());
                    Log.Debug($"[DispatcherActor] Become dispatching.");
                    Become(Dispatching);
                    Stash.UnstashAll();
                }
            });
            ReceiveAny(_ => Stash.Stash());
        }

        private void Dispatching()
        {
            Receive<StartMessage>(_ =>
            {
                Log.Debug("[DispatcherActor] Receive start message.");
                RequestQueueActorRef.Tell(new GetRequestMessage(this.hostActorCount));
            });
            Receive<List<RequestMessage>>(message =>
            {
                int requestCount = message.Count;
                foreach (RequestMessage request in message)
                {
                    try
                    {
                        var host = hostActorQueue.Dequeue();
                        host.Tell(request);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"[DispatcherActor] Error occur when dispatching request: {ex.Message}");
                    }
                }
            });

            Receive<RequestCompleteMessage>(_ =>
            {
                Log.Debug($"Get response from {Sender}");
                responseCount++;
                if (responseCount == totalRequestCount)
                {
                    Log.Debug("Get all response.");
                    ts.TrySetResult(true);
                }
                else
                {
                    hostActorQueue.Enqueue(Sender);
                    RequestQueueActorRef.Tell(new GetRequestMessage(1));
                }
            });
        }

        public IStash Stash { get; set; }
    }
}
