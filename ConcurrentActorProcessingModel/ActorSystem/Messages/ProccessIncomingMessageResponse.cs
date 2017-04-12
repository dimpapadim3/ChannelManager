namespace ConcurrentActorProcessingModel.ActorSystem.Messages
{
    public class ProccessIncomingMessageResponse
    {
        public ProccessIncomingMessage IncomingMessage { get; set; }
        public int MessageId { get; set; }
        public long QueuedMessageId { get; set; }
    }
}