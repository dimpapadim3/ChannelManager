using System;
using System.Threading;
using System.Threading.Tasks;
using ChannelManager.Domain;

namespace BookingComWebService
{
    public class  ChannelWebService : ISendingWebService<IncomingMessage>
    {
        public ChannelWebService()
        {
         }

        public void Activate()
        {
            Thread.Sleep(5000);
            if (OnReceivedNewIncomingMessage != null)
                OnReceivedNewIncomingMessage(this, new IncomingMessage
                {
                    ChannelName = "Booking.com",
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