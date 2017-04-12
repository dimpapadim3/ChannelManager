using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem
{
    public class ValidationActor : ReceiveActor
    {
        public ValidationActor()
        {
            Receive<ValidateIncomingMessage>(message => ValidateIncomingMessage(message));
        }

        private void ValidateIncomingMessage(ValidateIncomingMessage message)
        {
            Debug.WriteLine($"ValidationActor.ValidateIncomingMessage {message.QueuedMessageId}");

            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
                    var queuedMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
                    if (queuedMessage == null)
                        throw new Exception($"Message with id  {message.QueuedMessageId} could not be found");


                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"MessageCoordinatorActor.ProccessIncomingMessage  exception {e.Message}");
                Sender.Tell(new ProccessIncomingMessageResponse(), Self);
            }
        }

    }


}
