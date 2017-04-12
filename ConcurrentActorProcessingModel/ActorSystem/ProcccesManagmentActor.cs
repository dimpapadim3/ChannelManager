using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Util.Internal;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem
{
    /// <summary>
    ///     MessageCoordinatorActor accepts the incoming messages from the Channel Manager
    ///     and then use the actor model in order to process them concurrently
    /// </summary>
    public class ProcccesManagmentActor : ReceiveActor
    {
        //private static IActorRef _instance;
        public IChannelManagerService ChannelManagerService { get; set; }
        public IDictionary<string, IActorRef> ResgisteredWebServices = new Dictionary<string, IActorRef>
        {

        };
        private ValidationResult ValidateIncomingMessage(ValidateIncomingMessage message)
        {

            Debug.WriteLine($"ValidationActor.ValidateIncomingMessage {message.QueuedMessageId}");

            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
                    var queuedMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
                    if (queuedMessage == null)
                        throw new Exception($"Message with id  {message.QueuedMessageId} could not be found");
                    var incomingReservation = XmlConverter<IncomingMessage>.DeserializeFromXmlString(queuedMessage.Message);
                    var roomReservations = channelsUnitOfWork.ReservationRepository
                        .Get(reservation => (reservation.RoomId == incomingReservation.RoomId));

                    var reservations = roomReservations as IList<Reservation> ?? roomReservations.ToList();
                    var incomingReservationStartDateIsInAnExistingReservation = reservations.Any(reservation =>
                        (reservation.StartDate <= incomingReservation.StartDate)
                        && (incomingReservation.StartDate <= reservation.EndDate));

                    var incomingReservationEndDateIsInAnExistingReservation = reservations.Any(reservation =>
                        (reservation.StartDate <= incomingReservation.EndDate)
                        && (incomingReservation.EndDate <= reservation.EndDate));

                    if (incomingReservationStartDateIsInAnExistingReservation ||
                        incomingReservationEndDateIsInAnExistingReservation)
                    {
                        return new ValidationResult(message.QueuedMessageId, false);
                        // Sender.Tell(new ValidationResult(message.QueuedMessageId,false),Self);
                    }
                    else
                    {
                        // Sender.Tell(new ValidationResult(message.QueuedMessageId,true), Self);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"MessageCoordinatorActor.ProccessIncomingMessage  exception {e.Message}");
            }
            return new ValidationResult(message.QueuedMessageId, true);

        }

        public void ProcessIncomingMessage(IncomingMessage message)
        {
            Debug.WriteLine($"ChannelManagerService.ProccessIncomingMessage {message}");

            var channelsMessageQueueMessage = GetChannelsMessageQueueMessage(message);
            Debug.WriteLine($"ChannelManagerService.ProccessIncomingMessage channelsMessageQueueMessage.Message : {channelsMessageQueueMessage.Message}");
            using (var transaction = new TransactionScope())
            {

                TrySaveMessageToDataBase(channelsMessageQueueMessage);

                var validationResult =
                    ValidateIncomingMessage(new ValidateIncomingMessage(channelsMessageQueueMessage.MessageId));
                if (validationResult.IsValid)
                {
                    //synchronously update the database 
                    // UpdateDataBaseFromIncomingMessage(message);
                    UpdateDbMessage(new UpdateDbMessage(channelsMessageQueueMessage.MessageId));

                    //and notify coordination actor to send the updates to the remaining channels 
                    NotifyChannelsMessage(
                         new NotifyChannelsMessage(validationResult.QueuedMessageId, message.ChannelName));
                }
                else
                {
                    //if the message fails the validation e.g cannot make the transaction like reservation then send back to the 
                    //channel a cancellation notification 
                    CancellationIncomingRequestMessage(
                        new CancellationMessage(validationResult.QueuedMessageId, message.ChannelName));
                }
                transaction.Complete();
            }

        }


        public ProcccesManagmentActor()
        {
            RegisterWebServices();
            Receive<ValidateIncomingMessage>(message => ValidateIncomingMessage(message));
            Receive<CancellationMessage>(message => CancellationIncomingRequestMessage(message));
            Receive<UpdateDbMessage>(message => UpdateDbMessage(message));
            Receive<NotifyChannelsMessage>(message => NotifyChannelsMessage(message));
            Receive<IncomingMessageReceived>(message => IncomingMessageReceived(message));


        }

        protected override void PreStart()
        {
            var bookingComRervice = Context.ActorOf(Context.DI().Props<BookingComCommunicationActor>());
            var pmsService = Context.ActorOf(Context.DI().Props<PmsCommunicationActor>());
            var expediaComRervice = Context.ActorOf(Context.DI().Props<ExpediaComCommunicationActor>());

            ResgisteredWebServices.Add("Booking.com", bookingComRervice);
            ResgisteredWebServices.Add("PMS", pmsService);
            ResgisteredWebServices.Add("Expedia.com", expediaComRervice);

        }


        private void RegisterWebServices()
        {


        }

        private void IncomingMessageReceived(IncomingMessageReceived message)
        {
            ProcessIncomingMessage(message.IncomingMessage);
        }

        //public static IActorRef Instance
        //{
        //    get
        //    {
        //        return _instance ?? (_instance =
        //            ActorSystemReference.ActorSystem.ActorOf(Props.Create(() => new ProcccesManagmentActor())));
        //    }
        //}

        object _updateDblock = new object();
        private void UpdateDbMessage(UpdateDbMessage message)
        {
            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {


                    var queuedMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
                    if (queuedMessage == null)
                        throw new Exception($"Message with id  {message.QueuedMessageId} could not be found");
                    var incomingReservation = XmlConverter<IncomingMessage>.DeserializeFromXmlString(queuedMessage.Message);

                    var reservation = new Reservation
                    {
                        ReservationType = 1,
                        RoomId = incomingReservation.RoomId,
                        EndDate = incomingReservation.EndDate,
                        StartDate = incomingReservation.StartDate
                    };
                    channelsUnitOfWork.ReservationRepository.Insert(reservation);

                    channelsUnitOfWork.Save();
                    Debug.WriteLine(
                        $"ChannelManagerService.reservation Added for roomId {incomingReservation.RoomId}  " +
                        $"  Id {reservation.Id}");

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ChannelManagerService.UpdateDb  exception {e.Message}");
            }
        }
        object _lock = new object();
        /// <summary>
        /// Notifies the remaining registered channels for the changes that had been made by one channel.
        /// And persist the processing state
        /// </summary>
        /// <param name="message"></param>
        private void NotifyChannelsMessage(NotifyChannelsMessage message)
        {
            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                var queueMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
                queueMessage.ProccessingState = (int)ProcessingStates.NotifingChannels;
                channelsUnitOfWork.QueueMessageRepository.Update(queueMessage);
                var proccessingStateChanced = channelsUnitOfWork.Save();
                if (proccessingStateChanced == 1)
                {
                    var channelsToSendMessage = ResgisteredWebServices.Where(channel => channel.Key != message.ChannelId);
                    List<Task<SendNotificationResult>> sendNotificationResults = new List<Task<SendNotificationResult>>();
                    foreach (var channel in channelsToSendMessage)
                    {
                        Debug.WriteLine($"NotifyChannelsMessage channel {channel} QueuedMessageId {message.QueuedMessageId}");
                        var notificationResult = channel.Value.Ask<SendNotificationResult>(new SendNotificationMessage());
                        notificationResult.ContinueWith(notificationTask =>
                        {
                            using (var unitOfWork = new ChannelsUnitOfWork())
                            {

                                lock (_lock)
                                {
                                    var queuedMessage = unitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
                                    OverallNotificationState notificationState;
                                    if (!string.IsNullOrEmpty(queuedMessage.NotificationState))
                                        notificationState = XmlConverter<OverallNotificationState>
                                        .DeserializeFromXmlString(queuedMessage.NotificationState);
                                    else
                                    {
                                        notificationState = new OverallNotificationState();
                                    }
                                    if (notificationState.ChannelNotificationStates
                                    .All(state => state.ChannelId != notificationTask.Result.ChannelId))
                                    {
                                        Debug.WriteLine($" channel {notificationTask.Result.ChannelId} finished {message.QueuedMessageId}");
                                        notificationState.ChannelNotificationStates.Add(notificationTask.Result);
                                        queuedMessage.NotificationState =
                                      XmlConverter<OverallNotificationState>.SerializeToXml(notificationState);

                                        unitOfWork.QueueMessageRepository.Update(queuedMessage);
                                        proccessingStateChanced = unitOfWork.Save();
                                    }
                                }

                            }


                        });
                        sendNotificationResults.Add(notificationResult);
                    }
                    Task.WaitAll(sendNotificationResults.ToArray());
                    //Thread.Sleep(2000);
                    if (sendNotificationResults.All(results => results.Result.NotificationResultState == (int)SendNotificationResult.NotificationResultStates.Succeess))
                        queueMessage.ProccessingState = (int)ProcessingStates.NotifiededChannelsSuccesfully;
                    else
                        queueMessage.ProccessingState = (int)ProcessingStates.NotifiededChannelsErrorsOccured;

                    proccessingStateChanced = channelsUnitOfWork.Save();

                }


            }
        }

        //private void ValidateIncomingMessage(ValidateIncomingMessage message)
        //{
        //    Debug.WriteLine($"ValidationActor.ValidateIncomingMessage {message.QueuedMessageId}");
        //    var persistingTask = Task.Run(() =>
        //    {
        //        try
        //        {
        //            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
        //            {
        //                var queuedMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
        //                if (queuedMessage == null)
        //                    throw new Exception($"Message with id  {message.QueuedMessageId} could not be found");
        //                var incomingReservation = XmlConverter<IncomingMessage>.DeserializeFromXmlString(queuedMessage.Message);
        //                var roomReservations = channelsUnitOfWork.ReservationRepository
        //                    .Get(reservation => (reservation.Id == incomingReservation.RoomId));

        //                var incomingReservationStartDateIsInAnExistingReservation = roomReservations.Any(reservation =>
        //                    (reservation.StartDate <= incomingReservation.StartDate)
        //                    && (incomingReservation.StartDate <= reservation.EndDate));

        //                var incomingReservationEndDateIsInAnExistingReservation = roomReservations.Any(reservation =>
        //                    (reservation.StartDate <= incomingReservation.EndDate)
        //                    && (incomingReservation.EndDate <= reservation.EndDate));

        //                if (incomingReservationStartDateIsInAnExistingReservation ||
        //                    incomingReservationEndDateIsInAnExistingReservation)
        //                {
        //                    return new ValidationResult(message.QueuedMessageId, false);
        //                    // Sender.Tell(new ValidationResult(message.QueuedMessageId,false),Self);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.WriteLine($"MessageCoordinatorActor.ProccessIncomingMessage  exception {e.Message}");
        //        }
        //        return new ValidationResult(message.QueuedMessageId, true);
        //    });
        //    persistingTask.PipeTo(Sender, Self);
        //}
        private static ChannelsMessageQueueMessage GetChannelsMessageQueueMessage(IncomingMessage message)
        {
            var channelsMessageQueueMessage = new ChannelsMessageQueueMessage()
            {
                ChannelName = message.ChannelName,
                HotelID = message.HotelId,
                MessageType = (int)message.MessageType,
                IsProcceced = false,
                TimeReceived = DateTime.Now,
                Message = XmlConverter<IncomingMessage>.SerializeToXml(message)
            };
            return channelsMessageQueueMessage;
        }

        private static void TrySaveMessageToDataBase(ChannelsMessageQueueMessage channelsMessageQueueMessage)
        {
            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
                    channelsUnitOfWork.QueueMessageRepository.Insert(channelsMessageQueueMessage);
                    channelsUnitOfWork.Save();
                    Debug.WriteLine("ChannelManagerService.ProccessIncomingMessage Saved" +
                                    $"  MessageId: {channelsMessageQueueMessage.MessageId}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ChannelManagerService.ProccessIncomingMessage  exception {e.Message}");
            }
        }

        private void CancellationIncomingRequestMessage(CancellationMessage message)
        {
            if (ResgisteredWebServices.ContainsKey(message.ServiceId))
            {
                ResgisteredWebServices[message.ServiceId].Tell(message);
                Console.WriteLine($"canceling QueuedMessageId {message.QueuedMessageId}");
            }
            else throw new Exception($"Cannot find web Service for Channel with Id  {message.ServiceId}");
        }

        //            MessageType = message.MessageType,
        //            HotelID = message.HotelId,
        //        {
        //        var channelsMessageQueueMessage = new ChannelsMessageQueueMessage()
        //        Debug.WriteLine($"MessageCoordinatorActor.ProccessIncomingMessage {message.IncommingMessageId}");
        //    {
        //    var persistingTask = Task.Run(async () =>
        //{

        //private void ProccessIncomingMessage(ProccessIncomingMessage message)
        //            IsProcceced = false,
        //            TimeReceived = DateTime.Now,
        //        };
        //        try
        //        {
        //            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
        //            {
        //                channelsUnitOfWork.QueueMessageRepository.Insert(channelsMessageQueueMessage);
        //                var saveResult = await channelsUnitOfWork.SaveChangesAsync();
        //                Debug.WriteLine("MessageCoordinatorActor.ProccessIncomingMessage SaveChangesAsync" +
        //                                $" {saveResult} IncommingMessageId: {message.IncommingMessageId}");

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.WriteLine($"MessageCoordinatorActor.ProccessIncomingMessage  exception {e.Message}");
        //            Sender.Tell(new ProccessIncomingMessageResponse(), Self);
        //        }

        //        return new ProccessIncomingMessageResponse()
        //        {
        //            QueuedMessageId = channelsMessageQueueMessage.MessageId,
        //            IncomingMessage = message
        //        };
        //    });

        //    persistingTask.PipeTo(Sender, Self);
        //} 
    }

    public class OverallNotificationState
    {
        public List<SendNotificationResult> ChannelNotificationStates { get; set; }
            = new List<SendNotificationResult>();

    }

    public class SendNotificationResult
    {
        public string ChannelId { get; set; }
        public int NotificationResultState { get; set; }
        public string ErrorMessage { get; set; }
        public enum NotificationResultStates
        {
            Succeess, Failed,
        }
    }

    public class ValidationResult
    {
        public ValidationResult(long queuedMessageId, bool b)
        {
            QueuedMessageId = queuedMessageId;
            IsValid = b;
        }

        public long QueuedMessageId { get; set; }
        public bool IsValid { get; set; }
    }
}