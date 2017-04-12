namespace ConcurrentActorProcessingModel.ActorSystem.Messages
{
    public class CancellationMessage
    {
        public long QueuedMessageId { get; }
        public string ServiceId { get; set; }

        public CancellationMessage(long queuedMessageId, string channelId)
        {
            QueuedMessageId = queuedMessageId;
            ServiceId = channelId;

        }
    }
}