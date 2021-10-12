namespace Email.Sender.ServiceBus.Models.Enums
{
    public enum MessageProcessResponse
    {
        Complete = 0,
        ReEnqueue = 1,
        DeadLetter = 2
    }
}