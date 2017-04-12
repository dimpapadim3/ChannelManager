using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem.ProccesStates
{
    public class ValidationState : State
    {
        public override int StateEnumIdEntering { get; set; } = (int)ProcessingStates.StartValidation;
        public override int StateEnumIdExitingSuccesfully { get; set; } = (int)ProcessingStates.Validated;

        private IStateContext _stateContext;
        public ValidationState(StateValidationParameters validationParameters)
        {
            ValidationParameters = validationParameters;
            QueuedMessageId = validationParameters.ChannelsMessageQueueMessage.MessageId;
        }
        public StateValidationParameters ValidationParameters { get; set; }
        public override void Proccess(IStateContext stateContext)
        {
            _stateContext = stateContext;
            ChannelsMessageQueueMessage channelsMessageQueueMessage = ValidationParameters
                .ChannelsMessageQueueMessage;
            var validationResult = Validate(channelsMessageQueueMessage);

            if (validationResult.IsValid && !validationResult.HasException)
            {
                _stateContext.Log.Debug($"validationResult.IsValid && !validationResult.HasException {channelsMessageQueueMessage.MessageId}");

                //update the database 
                _stateContext.TransitionToState(
                    new UpdateDataBaseState(
                        new UpdateDbMessageParameters(channelsMessageQueueMessage.MessageId)));

            }
            else if (!validationResult.IsValid && !validationResult.HasException)
            {
                _stateContext.Log.Debug($"!validationResult.IsValid && !validationResult.HasException {channelsMessageQueueMessage.MessageId}");

                //if the message fails the validation e.g cannot make the transaction like reservation then send back to the 
                //channel a cancellation notification 
                _stateContext.TransitionToState(
                    new CancellationState(
                        new NotificationStateParameters(channelsMessageQueueMessage.MessageId,
                        channelsMessageQueueMessage.ChannelName)));
            }
            else if (validationResult.HasException)
            {
                _stateContext.Log.Debug($"validationResult.HasException {channelsMessageQueueMessage.MessageId}");

                //if the message fails the validation due to exception retry 
                //ValidationState1(message, channelsMessageQueueMessage);
            }

        }

        private ValidationResult Validate(ChannelsMessageQueueMessage queuedMessage)
        {
            _stateContext.Log.Debug($"ValidationState.Validate queuedMessage: {queuedMessage}");

            try
            {
                var validateIncomingMessage = new ValidationResult(queuedMessage.MessageId);

                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
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
                        if (incomingReservationStartDateIsInAnExistingReservation)
                            validateIncomingMessage.Message += $"incoming Reservation StartDate Is In An Existing Reservation  ";
                        if (incomingReservationEndDateIsInAnExistingReservation)
                            validateIncomingMessage.Message += "incoming Reservation EndDate Is In An Existing Reservation";
                        validateIncomingMessage.IsValid = false;

                        _stateContext.Log.Debug($"    validateIncomingMessage.IsValid = false;");

                        return validateIncomingMessage;
                    }
                    else
                    {
                        _stateContext.Log.Debug($"    validateIncomingMessage.IsValid = true;");
                        validateIncomingMessage.IsValid = true;
                        return validateIncomingMessage;
                    }
                }
            }
            catch (Exception e)
            {
                _stateContext.Log.Debug($"MessageCoordinatorActor.ProccessIncomingMessage  exception {e.Message}");
                var validationResult = new ValidationResult(queuedMessage.MessageId, false)
                {
                    HasException = true,
                    Message = e.Message
                };
                return validationResult;
            }
        }

        //private void UpdateDbMessage(UpdateDbMessage message)
        //{
        //    try
        //    {
        //        using (var channelsUnitOfWork = new ChannelsUnitOfWork())
        //        {
        //            var queuedMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(message.QueuedMessageId);
        //            if (queuedMessage == null)
        //                throw new Exception($"Message with id  {message.QueuedMessageId} could not be found");
        //            var incomingReservation = XmlConverter<IncomingMessage>.DeserializeFromXmlString(queuedMessage.Message);

        //            var reservation = new Reservation
        //            {
        //                ReservationType = 1,
        //                RoomId = incomingReservation.RoomId,
        //                EndDate = incomingReservation.EndDate,
        //                StartDate = incomingReservation.StartDate
        //            };
        //            channelsUnitOfWork.ReservationRepository.Insert(reservation);

        //            channelsUnitOfWork.Save();
        //            Debug.WriteLine(
        //                $"ChannelManagerService.reservation Added for roomId {incomingReservation.RoomId}  " +
        //                $"  Id {reservation.Id}");

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine($"ChannelManagerService.UpdateDb  exception {e.Message}");
        //    }
        //}

    }
}