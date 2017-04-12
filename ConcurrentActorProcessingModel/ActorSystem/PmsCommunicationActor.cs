using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem
{
    public class PmsCommunicationActor : ReceiveActor
    {
        public ISendingWebService<IncomingMessage> PmsWebService { get; set; }

        public PmsCommunicationActor()
        {
            PmsWebService = new PmsWebService.ChannelWebService();
            PmsWebService.OnReceivedNewIncomingMessage += ReceiveNotificationFromWebService;
            PmsWebService.Activate();
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
                Console.WriteLine($"PmsWebServiceActor SendNotificationMessage {message}");
                Thread.Sleep(3000);
                return new SendNotificationResult()
                {
                    ChannelId = "PMS",
                    NotificationResultState = (int)SendNotificationResult.NotificationResultStates.Failed,
                    ErrorMessage = "Pms Notification Failed"
                };
            });
            persistingTask.PipeTo(Sender, Self);
            // return persistingTask;
        }
        public void SendCancellationMessage(CancellationMessage message)
        {
            Console.WriteLine($"WebServiceActor canceling QueuedMessageId {message.QueuedMessageId}");
        }
    }
}
