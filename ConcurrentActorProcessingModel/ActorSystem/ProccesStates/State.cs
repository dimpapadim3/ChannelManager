using System;
using System.Text;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;

namespace ConcurrentActorProcessingModel.ActorSystem.ProccesStates
{
    public class StateValidationParameters
    {
        public long QueuedMessageId { get; set; }
        public IncomingMessage Message { get; set; }
        public ChannelsMessageQueueMessage ChannelsMessageQueueMessage { get; set; }

        public StateValidationParameters(IncomingMessage message, ChannelsMessageQueueMessage channelsMessageQueueMessage)
        {
            Message = message;
            ChannelsMessageQueueMessage = channelsMessageQueueMessage;
        }
         
    }

    public abstract class State
    {
        public long QueuedMessageId { get; set; }

        public virtual void Proccess(IStateContext validationParameters)
        {

        }

        public void LogExiting()
        {
            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                channelsUnitOfWork.ProccesStateTransitionsRepository.Insert(new ProccesStateTransitions
                {
                    QueuedMessageId = QueuedMessageId,
                    TimeTransitioned = DateTime.Now,
                    StateId = this.StateEnumIdExitingSuccesfully,

                });
                channelsUnitOfWork.Save();
            }
        }

        public abstract int StateEnumIdEntering { get; set; }
        public abstract int StateEnumIdExitingSuccesfully { get; set; }

        public void LogEntering()
        {
            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                channelsUnitOfWork.ProccesStateTransitionsRepository.Insert(new ProccesStateTransitions
                {
                    QueuedMessageId = QueuedMessageId,
                    TimeTransitioned = DateTime.Now,
                    StateId = this.StateEnumIdExitingSuccesfully,

                }); channelsUnitOfWork.Save();
            }
        }
    }
}
