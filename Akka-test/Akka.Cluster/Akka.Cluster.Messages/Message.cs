using System;

namespace Akka.Cluster.Messages
{
    public class StartMessage
    {
        public StartMessage()
        {

        }
    }

    public class RequestMessage
    {
        public string Jobid { get; set; }
        public RequestMessage(int id)
        {
            this.Jobid = id.ToString();
        }
    }

    public class RequestCompleteMessage
    {
        public RequestCompleteMessage()
        {

        }
    }
}
