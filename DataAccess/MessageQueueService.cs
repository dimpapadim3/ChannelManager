using System;
using ChannelManager.Domain;

namespace ChannelManagerService.DataAccess
{
    public class Consumer
    {
        public event EventHandler<ChannelsMessageQueueMessage> Receive;

        public virtual void OnReceive(ChannelsMessageQueueMessage e)
        {
            Receive?.Invoke(this, e);
        }
    }
    public interface IMessageQueueService
    {
        void Publish(ChannelsMessageQueueMessage incomingMessage);
        void Consume(Consumer consumer);
    }

    public class SimpleSqlMessageQueueService : IMessageQueueService
    {
        private Consumer _consumer;

        public void Publish(ChannelsMessageQueueMessage incomingMessage)
        {
            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                channelsUnitOfWork.QueueMessageRepository.Insert(incomingMessage);
                channelsUnitOfWork.Save();
                _consumer.OnReceive(incomingMessage);
            }
        }

        public void Consume(Consumer consumer)
        {
            this._consumer = consumer;
        }
 
    }
}
