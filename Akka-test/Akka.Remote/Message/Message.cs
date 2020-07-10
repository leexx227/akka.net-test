using System;

namespace ZKB.Messages
{
    public class RequestMessage
    {
        public byte[] Message { get; set; }
        public RequestMessage(int messageLength)
        {
            this.Message = new byte[messageLength];
            (new Random()).NextBytes(this.Message);
        }
    }

    public class GetRequestMessage
    {
        public int Num { get; set; }
        public GetRequestMessage(int num)
        {
            this.Num = num;
        }
    }

    public class RequestCompleteMessage
    {
        public RequestCompleteMessage()
        {

        }
    }

    public class HostReadyMessage
    {
        public HostReadyMessage()
        {

        }
    }

    public class StartMessage
    {
        public StartMessage()
        {

        }
    }

}
