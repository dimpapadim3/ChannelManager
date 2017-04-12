using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Event;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem;
using ConcurrentActorProcessingModel.ActorSystem.Messages;
using ConcurrentActorProcessingModel.ActorSystem.ProccesStates;
using Debug = System.Diagnostics.Debug;

namespace ConcurrentActorProcessingModel
{
    public class ProccessManagementStatefullActor : ReceiveActor, IStateContext
    {
        public ILoggingAdapter Log { get; } = Logging.GetLogger(Context);
        private ICancelable _ErrorStatesRestoration;

        public State State { get; set; }
        public void TransitionToState(State newState)
        {
            Log.Info($"Transitioning to state {newState.GetType()}");
            State?.LogExiting();

            State = newState;

            if (State != null)
            {
                State.LogEntering();
                State.Proccess(this);
            }
        }

        public IDictionary<string, IActorRef> ResgisteredWebServices { get; set; } = new Dictionary<string, IActorRef>();

        public ProccessManagementStatefullActor()
        {
            Receive<IncomingMessageReceived>(message => ProcessIncomingMessage(message.IncomingMessage));
            Receive<ResendNotificationMessage>(message => RestoreFaultNotificationStates(message.FailedNotifications));

             
        }

        private void RestoreFaultNotificationStates(IEnumerable<NotificationMessageState> failedNotifications)
        { 

        }

        protected override void PreStart()
        {
            var bookingComRervice = Context.ActorOf(Context.DI().Props<BookingComCommunicationActor>());
            var pmsService = Context.ActorOf(Context.DI().Props<PmsCommunicationActor>());
            var expediaComRervice = Context.ActorOf(Context.DI().Props<ExpediaComCommunicationActor>());

            ResgisteredWebServices.Add("Booking.com", bookingComRervice);
            ResgisteredWebServices.Add("PMS", pmsService);
            ResgisteredWebServices.Add("Expedia.com", expediaComRervice);

            ProccessRestorationActor = Context.ActorOf(Context.DI().Props<ProccessRestorationActor>());
            _ErrorStatesRestoration = Context.System
               .Scheduler
               .ScheduleTellRepeatedlyCancelable(
                   TimeSpan.FromSeconds(1),
                   TimeSpan.FromSeconds(1),
                   ProccessRestorationActor,
                   new RestoreMessagesInFaultStatesMessage(),
                   Self);

        }

        public IActorRef ProccessRestorationActor { get; set; }

        public void ProcessIncomingMessage(IncomingMessage message)
        {
            Log.Debug($"ProccessManagementStatefullActor.ProccessIncomingMessage {message}");

            var channelsMessageQueueMessage = new ChannelsMessageQueueMessage()
            {
                ChannelName = message.ChannelName,
                HotelID = message.HotelId,
                MessageType = (int)message.MessageType,
                IsProcceced = false,
                TimeReceived = DateTime.Now,
                Message = XmlConverter<IncomingMessage>.SerializeToXml(message)
            };
            Log.Info($"Process IncomingMessage :{channelsMessageQueueMessage.Message}");

            using (var transaction = new TransactionScope())
            {
                var messageQueueMessageSaveResult = TrySaveMessageToDataBase(channelsMessageQueueMessage);

                State validationState = new ValidationState(
                    new StateValidationParameters(message,
                    channelsMessageQueueMessage));

                this.TransitionToState(validationState);
                transaction.Complete();
            }

        }

        private MessageQueuePersistResult TrySaveMessageToDataBase(ChannelsMessageQueueMessage channelsMessageQueueMessage)
        {
            Log.Debug($"ProccessManagementStatefullActor.TrySaveMessageToDataBase {channelsMessageQueueMessage}");

            var messageQueuePersistResult = new MessageQueuePersistResult();
            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
                    channelsUnitOfWork.QueueMessageRepository.Insert(channelsMessageQueueMessage);
                    var saveResult = channelsUnitOfWork.Save();

                    Log.Debug($"messageQueuePersistResult.SavedSuccessfully {saveResult}");

                    if (saveResult.UpdatedEntities > 0)
                    {
                        messageQueuePersistResult.SavedSuccessfully = true;

                    }
                    Log.Info("IncomingMessage Saved" +
                                    $" with MessageId: {channelsMessageQueueMessage.MessageId}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"ChannelManagerService.ProccessIncomingMessage  exception {e.Message}");
                messageQueuePersistResult.SavedSuccessfully = false;

            }

            return messageQueuePersistResult;
        }


    }

}
