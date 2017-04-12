using System;
using System.Diagnostics;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem.ProccesStates
{
    public class UpdateDbMessageParameters
    {
        public long QueuedMessageId { get; set; }

        public UpdateDbMessageParameters(long queuedMessageId)
        {
            QueuedMessageId = queuedMessageId;
        }
    }

    public class UpdateDataBaseState : State
    {
        private IStateContext _stateContext;
        public UpdateDbMessageParameters UpdateDbMessageParameters { get; set; }
        public override int StateEnumIdEntering { get; set; } = (int)ProcessingStates.StartValidation;
        public override int StateEnumIdExitingSuccesfully { get; set; } = (int)ProcessingStates.Validated;

        public UpdateDataBaseState(UpdateDbMessageParameters updateDbMessageParameters)
        {
            QueuedMessageId = updateDbMessageParameters.QueuedMessageId;
            UpdateDbMessageParameters = updateDbMessageParameters;
        }

        public override void Proccess(IStateContext context)
        {
            _stateContext = context;

            try
            {
                using (var channelsUnitOfWork = new ChannelsUnitOfWork())
                {
                    var queuedMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(UpdateDbMessageParameters.QueuedMessageId);
                    if (queuedMessage == null)
                        throw new Exception($"Message with id  {UpdateDbMessageParameters.QueuedMessageId} could not be found");
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

                    _stateContext.TransitionToState(new NotificationState(
                     new NotificationStateParameters(UpdateDbMessageParameters.QueuedMessageId,
                     queuedMessage.ChannelName)));

                    _stateContext.Log.Info(
                        $" reservation Added for roomId {incomingReservation.RoomId}  " +
                        $"  Id {reservation.Id} by message with id  { QueuedMessageId}");

                }
            }
            catch (Exception e)
            {
                _stateContext.Log.Error($"ChannelManagerService.UpdateDb  exception {e.Message}");
            }
        }


    }

}