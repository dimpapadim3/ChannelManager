using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem.ProccesStates
{
    public class NotificationStateParameters
    {
        public long QueuedMessageId { get; set; }
        public string ChannelId { get; set; }

        public NotificationStateParameters(long queuedMessageId, string channelName)
        {
            QueuedMessageId = queuedMessageId;
            ChannelId = channelName;
        }
    }
    public class SingleChannelNotificationStateParameters
    {
        public long QueuedMessageId { get; set; }
        public string ChannelId { get; set; }

        public SingleChannelNotificationStateParameters(long queuedMessageId, string channelName)
        {
            QueuedMessageId = queuedMessageId;
            ChannelId = channelName;
        }
    }
    public class NotificationState : State
    {
        private IStateContext _stateContext;
        public override int StateEnumIdEntering { get; set; } = (int)ProcessingStates.NotifingChannels;
        public override int StateEnumIdExitingSuccesfully { get; set; } = (int)ProcessingStates.NotifiededChannels;

        public NotificationState(NotificationStateParameters notificationStateParameters)
        {
            QueuedMessageId = notificationStateParameters.QueuedMessageId;
            NotificationParameters = notificationStateParameters;
        }

        NotificationStateParameters NotificationParameters { get; set; }
        public override void Proccess(IStateContext context)
        {
            _stateContext = context;

            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                var queueMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(NotificationParameters.QueuedMessageId);

                var channelsToSendMessage = _stateContext.ResgisteredWebServices
                    .Where(channel => channel.Key != NotificationParameters.ChannelId);

                List<Task<SendNotificationResult>> sendNotificationResults
                    = new List<Task<SendNotificationResult>>();

                foreach (var channel in channelsToSendMessage)
                {
                    _stateContext.Log.Debug($"NotifyChannelsMessage channel {channel}" +
                                            $" QueuedMessageId {NotificationParameters.QueuedMessageId}");

                    var notificationResult = channel.Value
                        .Ask<SendNotificationResult>(new SendNotificationMessage(queueMessage));

                    notificationResult.ContinueWith(notificationTask =>
                    {
                        using (var unitOfWork = new ChannelsUnitOfWork())
                        {
                            unitOfWork.NotificationMessageStateRepository.Insert(new NotificationMessageState()
                            {
                                QueuedMessageId = QueuedMessageId,
                                StateId = notificationTask.Result.NotificationResultState,
                                TimeEntered = DateTime.Now,
                            });
                            unitOfWork.Save();
                        } 
                    });
                    sendNotificationResults.Add(notificationResult);
                }
       
            }
        } 
    }
    public class NotificationSingleChannelState : State
    {
        private IStateContext _stateContext;
        public override int StateEnumIdEntering { get; set; } = (int)ProcessingStates.NotifingChannels;
        public override int StateEnumIdExitingSuccesfully { get; set; } = (int)ProcessingStates.NotifiededChannels;

        public NotificationSingleChannelState(SingleChannelNotificationStateParameters notificationStateParameters)
        {
            QueuedMessageId = notificationStateParameters.QueuedMessageId;
            NotificationParameters = notificationStateParameters;
        }

        SingleChannelNotificationStateParameters NotificationParameters { get; }
        public override void Proccess(IStateContext context)
        {
            _stateContext = context;

            using (var channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                var queueMessage = channelsUnitOfWork.QueueMessageRepository.GetByID(NotificationParameters.QueuedMessageId);

                var channelToMessage = _stateContext.ResgisteredWebServices
                    .First(channel => channel.Key == NotificationParameters.ChannelId);
 
 
                    _stateContext.Log.Debug($"NotifyChannelsMessage channel {channelToMessage}" +
                                            $" QueuedMessageId {NotificationParameters.QueuedMessageId}");

                    var notificationResult = channelToMessage.Value
                        .Ask<SendNotificationResult>(new SendNotificationMessage(queueMessage));

                    notificationResult.ContinueWith(notificationTask =>
                    {
                        using (var unitOfWork = new ChannelsUnitOfWork())
                        {
                            unitOfWork.NotificationMessageStateRepository.Insert(new NotificationMessageState()
                            {
                                QueuedMessageId = QueuedMessageId,
                                StateId = notificationTask.Result.NotificationResultState,
                                TimeEntered = DateTime.Now,
                            });
                            unitOfWork.Save();
                        }
                    });
                   
                // Task.WaitAll(sendNotificationResults.ToArray());
                // if (sendNotificationResults.All(results => results.Result.NotificationResultState ==
                //(int)SendNotificationResult.NotificationResultStates.Succeess))
                //     queueMessage.ProccessingState = (int)ProcessingStates.NotifiededChannelsSuccesfully;
                // else
                //     queueMessage.ProccessingState = (int)ProcessingStates.NotifiededChannelsErrorsOccured;

                // channelsUnitOfWork.QueueMessageRepository.Update(queueMessage); 
            }
        }
    }
}