namespace ConcurrentActorProcessingModel.ActorSystem.Messages
{
    public class UpdateDbMessage
    {
        public long QueuedMessageId { get; set; }

        public UpdateDbMessage(long queuedMessageId)
        {
            QueuedMessageId = queuedMessageId;
         }
    }
}