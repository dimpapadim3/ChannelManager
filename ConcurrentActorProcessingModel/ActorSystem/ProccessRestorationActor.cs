using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.ProccesStates;

namespace ConcurrentActorProcessingModel.ActorSystem
{
    public class ProccessRestorationActor : ReceiveActor
    {
        public ProccessRestorationActor()
        {
            Receive<RestoreMessagesInFaultStatesMessage>(message => LookForMessagesInFaultStatesMessage(message));

        }

        private void LookForMessagesInFaultStatesMessage(RestoreMessagesInFaultStatesMessage message)
        {
            using (var unitOfWork = new ChannelsUnitOfWork())
            {
                var failedNotifications = unitOfWork.NotificationMessageStateRepository.Get(
                    notification => notification.StateId == (int)SendNotificationResult.NotificationResultStates.Failed);

                if (failedNotifications.Any())
                    Context.Parent.Tell(new ResendNotificationMessage(failedNotifications));


                var faultStates = unitOfWork.ProccesStateTransitionsRepository.Get(state => state.IsInFaultState);
                if (faultStates.Any())
                    Context.Parent.Tell(new RestoreProccessStateMessage(faultStates));



            }

        }
    }

    internal class RestoreProccessStateMessage
    {
        public IEnumerable<ProccesStateTransitions> FailedNotifications { get; set; }

        public RestoreProccessStateMessage(IEnumerable<ProccesStateTransitions> failedNotifications)
        {
            FailedNotifications = failedNotifications;
        }
    }

    internal class ResendNotificationMessage
    {
        public IEnumerable<NotificationMessageState> FailedNotifications { get; set; }

        public ResendNotificationMessage(IEnumerable<NotificationMessageState> failedNotifications)
        {
            FailedNotifications = failedNotifications;
        }
    }
}
