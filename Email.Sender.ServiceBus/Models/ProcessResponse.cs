using Email.Sender.ServiceBus.Models.Enums;

namespace Email.Sender.ServiceBus.Models
{
    public class ProcessResponse
    {
        public MessageProcessResponse Status { get; private set; }
        public string Message { get; private set; }

        public ProcessResponse(MessageProcessResponse status, string message = "")
        {
            Status = status;
            Message = message;
        }
    }
}