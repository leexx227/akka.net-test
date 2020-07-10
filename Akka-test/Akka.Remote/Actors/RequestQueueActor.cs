using Akka.Actor;
using ZKB.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using Akka.Event;

namespace ZKB.Actors
{
    public class RequestQueueActor : ReceiveActor
    {
        private int totalRequestCount;

        private int sendRequestCount = 0;

        private int messageLength;

        protected ILoggingAdapter Log { get; } = Context.GetLogger();
        public RequestQueueActor(int totalRequestCount, int messageLength)
        {
            this.totalRequestCount = totalRequestCount;
            this.messageLength = messageLength;

            Ready();
        }

        private void Ready()
        {
            Receive<GetRequestMessage>(message =>
            {
                Log.Debug($"[RequestQueueActor] receive GetRequestMessage.");
                if (sendRequestCount < totalRequestCount)
                {
                    int actualSendRequestCount = message.Num <= (totalRequestCount - sendRequestCount) ? message.Num : (totalRequestCount - sendRequestCount);
                    var messageList = new List<RequestMessage>();
                    for (var i = 0; i < actualSendRequestCount; i++)
                    {
                        messageList.Add(new RequestMessage(this.messageLength));
                    }
                    Sender.Tell(messageList);
                    sendRequestCount += actualSendRequestCount;
                    Log.Debug($"[RequestQueueActor] send {actualSendRequestCount} request.");
                }
                else
                {
                    Log.Debug("[RequestQueueActor] No more request remained.");
                }
            });
        }
    }
}
