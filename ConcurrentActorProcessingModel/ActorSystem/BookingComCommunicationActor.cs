using System;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManager.Domain;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem
{
    public class BookingComCommunicationActor : ReceiveActor
    {
        public ISendingWebService<IncomingMessage> BookingComWebService { get; set; }
         public BookingComCommunicationActor()
        {
            BookingComWebService = new BookingComWebService.ChannelWebService();
            BookingComWebService.OnReceivedNewIncomingMessage += ReceiveNotificationFromWebService;
            BookingComWebService.Activate();
            Receive<CancellationMessage>(message => SendCancellationMessage(message));
            Receive<SendNotificationMessage>(message => SendNotificationMessage(message));

        }

        private void ReceiveNotificationFromWebService(object sender, IncomingMessage e)
        {
            Context.Parent.Tell(new IncomingMessageReceived(e));
        }

        private void SendNotificationMessage(SendNotificationMessage message)
        {
            var persistingTask = Task.Run(async () =>
            {
                Console.WriteLine($"BookingComWebServiceActor SendNotificationMessage {message}");

                //await  BookingComWebService.SendNotificationMessage();
                return new SendNotificationResult()
                {
                    ChannelId = "Booking.com",
                    NotificationResultState = (int)SendNotificationResult.NotificationResultStates.Succeess,
                };
            });
            persistingTask.PipeTo(Sender, Self);
            //return persistingTask;
        }

        public void SendCancellationMessage(CancellationMessage message)
        {
            BookingComWebService.SendCancellationMessage(message);
            Console.WriteLine($"WebServiceActor canceling QueuedMessageId {message.QueuedMessageId}");

        }
    }

    public class IncomingMessageReceived
    {
        public IncomingMessage IncomingMessage { get; set; }

        public IncomingMessageReceived(IncomingMessage incomingMessage)
        {
            IncomingMessage = incomingMessage;
        }
    }
}