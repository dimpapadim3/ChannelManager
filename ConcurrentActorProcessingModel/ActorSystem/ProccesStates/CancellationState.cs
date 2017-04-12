using System;
using System.Diagnostics;
using Akka.Actor;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem.ProccesStates
{
    public class CancellationState : State
    {
        public override int StateEnumIdEntering { get; set; } = (int)ProcessingStates.SendingCancellationMessage;
        public override int StateEnumIdExitingSuccesfully { get; set; } = (int)ProcessingStates.SendedCancellationMessageSuccesfully;

        private IStateContext _context;
        public NotificationStateParameters NotificationParameters { get; set; }

        public CancellationState(NotificationStateParameters notificationStateParameters)
        {
            QueuedMessageId = notificationStateParameters.QueuedMessageId;
            NotificationParameters = notificationStateParameters;
        }

        public override void Proccess(IStateContext context)
        {
            _context = context;
            if (_context.ResgisteredWebServices.ContainsKey(NotificationParameters.ChannelId))
            {
                _context.Log.Debug($"canceling QueuedMessageId {NotificationParameters.QueuedMessageId}");

                var channel = _context.ResgisteredWebServices[NotificationParameters.ChannelId];

                var notificationResult =
                    channel.Ask<CancellationNotificationResult>(
                        new CancellationMessage(NotificationParameters.QueuedMessageId,
                            NotificationParameters.ChannelId));

                notificationResult.ContinueWith(notificationTask =>
                {
                    using (var unitOfWork = new ChannelsUnitOfWork())
                    {
                        unitOfWork.NotificationMessageStateRepository.Insert(new NotificationMessageState()
                        {
                            IsCancellation = true,
                            QueuedMessageId = QueuedMessageId,
                            StateId = notificationTask.Result.CancellationResultState,
                            TimeEntered = DateTime.Now,
                        });
                        unitOfWork.Save(); 
                    } 
                }); 
            }

        }
    }
}