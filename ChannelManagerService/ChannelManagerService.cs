using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Akka.Actor;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ChannelManagerService
{

    public class ChannelManagerService : IChannelManagerService
    {
        IActorRef ProcccesCoordinatior { get; set; }

        public ChannelManagerService()
        {
            //ProcccesCoordinatior = ProcccesManagmentActor.Instance; 
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
                    ProcccesCoordinatior.Tell(new UpdateDbMessage(channelsMessageQueueMessage.MessageId));

                    //and notify coordination actor to send the updates to the remaining channels 
                    ProcccesCoordinatior.Tell(
                          new NotifyChannelsMessage(validationResult.QueuedMessageId, message.ChannelName));
                }
                else
                {
                    //if the message fails the validation e.g cannot make the transaction like reservation then send back to the 
                    //channel a cancellation notification 
                    ProcccesCoordinatior.Tell(
                        new CancellationMessage(validationResult.QueuedMessageId, message.ChannelName));
                }
                transaction.Complete();
            }

        }

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

        private void UpdateDataBaseFromIncomingMessage(IncomingMessage message)
        {
            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
                    var reservation = new Reservation()
                    {
                        ReservationType = 1,
                        RoomId = message.RoomId,
                        EndDate = message.EndDate,
                        StartDate = message.StartDate,
                    };
                    channelsUnitOfWork.ReservationRepository.Insert(reservation);

                    channelsUnitOfWork.Save();
                    Debug.WriteLine($"ChannelManagerService.reservation Added for roomId {message.RoomId}  " +
                                    $"  Id {reservation.Id}");

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ChannelManagerService.UpdateDb  exception {e.Message}");
            }
        }

       

        //public void CheckCancellationRequest(IncomingMessage incomingMessage)
        //{
        //    using (var channelsUnitOfWork = new ChannelsUnitOfWork())
        //    {
        //        var currentProccessingMessages = channelsUnitOfWork.QueueMessageRepository
        //            .Get(m => m.HotelID == incomingMessage.HotelId && !m.IsProcceced);

        //        if (currentProccessingMessages.Any())
        //        {
        //            CancellationResults cancellationResult = SendCancellationRequest(incomingMessage);
        //            if (cancellationResult == CancellationResults.CancellationSucceded)
        //            {

        //            }
        //        }
        //    }
        //}

    }


}
