using System.ComponentModel.Design;

namespace ConcurrentActorProcessingModel.ActorSystem.Messages
{
    public class ValidateIncomingMessage
    {
        public ValidateIncomingMessage(long queuedMessageId)
        {
            QueuedMessageId = queuedMessageId;
        }

        public long QueuedMessageId {get;private set;}
    }
}