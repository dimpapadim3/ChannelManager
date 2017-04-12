using System;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManager.Domain;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem
{
    public class ExpediaComCommunicationActor : ReceiveActor
    {
        public ISendingWebService<IncomingMessage> BookingComWebService { get; set; }

        public ExpediaComCommunicationActor()
        {
            Receive<CancellationMessage>(message => SendCancellationMessage(message));
            Receive<SendNotificationMessage>(message => SendNotificationMessage(message));

        }

        private void SendNotificationMessage(SendNotificationMessage message)
        {
            var persistingTask = Task.Run(async () =>
            {
                Console.WriteLine($"ExpediaComWebServiceActor SendNotificationMessage {message}");

                //await  BookingComWebService.SendNotificationMessage();
                return new SendNotificationResult() { ChannelId = "Expedia.com",
                    NotificationResultState = (int)SendNotificationResult.NotificationResultStates.Succeess,
                };
            });
            persistingTask.PipeTo(Sender, Self);
            //return persistingTask;
        }

        public void SendCancellationMessage(CancellationMessage message)
        {
            Console.WriteLine($"WebServiceActor canceling QueuedMessageId {message.QueuedMessageId}");

        }
    }
}