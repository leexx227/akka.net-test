using Akka.Actor;
using Akka.Cluster.Messages;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Akka.Cluster.Actors
{
    public class BackendActor : ReceiveActor
    {
        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        public BackendActor()
        {
            Ready();
        }

        private void Ready()
        {
            Receive<RequestMessage>(message =>
            {
                Log.Info($"[Backend: {Context.Self}] receive request {message.Jobid} from {Sender}");
                Sender.Tell(new RequestCompleteMessage());
            });
        }

        protected override void PreStart()
        {
            Log.Info($"[Backend: {Context.Self}] start. Parent: {Context.Parent}");
        }
    }
}
