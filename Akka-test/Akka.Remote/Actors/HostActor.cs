using Akka.Actor;
using System;
using ZKB.Messages;
using System.Threading;
using System.Diagnostics;
using Akka.Event;

namespace ZKB.Actors
{
    public class HostActor : ReceiveActor
    {
        int requestTimeMilisec;

        string dispatcherPath = "akka.tcp://ZKB@TelepathyHN:2551/user/dispatcher";

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        public HostActor(int requestTime)
        {
            this.requestTimeMilisec = requestTime;

            Ready();
        }

        private void Ready()
        {
            Receive<RequestMessage>(message =>
            {
                Log.Debug($"HostActor receive request message.");
                var sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < this.requestTimeMilisec)
                {

                }
                sw.Stop();
                Sender.Tell(new RequestCompleteMessage());
                Log.Debug($"HostActor finish request and send response.");
            });
        }

        protected override void PreStart()
        {
            Log.Info($"HostActor: {Context.Self} start. Parent: {Context.Parent}");
            Context.Parent.Tell(new HostReadyMessage());
            //Context.ActorSelection(dispatcherPath).Tell(new HostReadyMessage());
        }
    }
}
