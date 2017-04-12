namespace ConcurrentActorProcessingModel.ActorSystem.Messages
{
    public class NotifyChannelsMessage
    {
        public long QueuedMessageId { get; set; }
        public string ChannelId { get; set; }

        public NotifyChannelsMessage(long queuedMessageId, string channelId)
        {
            QueuedMessageId = queuedMessageId;
            ChannelId = channelId;
        }
    }
}