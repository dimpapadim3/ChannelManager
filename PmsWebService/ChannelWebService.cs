using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChannelManager.Domain;

namespace PmsWebService
{
    public class ChannelWebService : ISendingWebService<IncomingMessage>
    {
        public ChannelWebService()
        {
        }

        public void Activate()
        {
            Thread.Sleep(5010);
            if (OnReceivedNewIncomingMessage != null)
                OnReceivedNewIncomingMessage(this, new IncomingMessage
                {
                    ChannelName = "PMS",
                    RoomId = 15058,
                    MessageType = IncomingChannelMessageType.Reserve,
                    StartDate = new DateTime(2016, 1, 24),
                    EndDate = new DateTime(2016, 1, 26),
                });
        }

        public void SendToChannelsMessage()
        {

        }

        public void SendCancellationMessage(object message)
        {

        }

        public event EventHandler<IncomingMessage> OnReceivedNewIncomingMessage;

        public Task SendNotificationMessage()
        {
            throw new NotImplementedException();
        }
    }
}
