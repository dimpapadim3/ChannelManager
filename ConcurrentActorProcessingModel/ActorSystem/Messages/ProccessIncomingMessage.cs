using ChannelManager.Domain;
using ChannelManagerService;

namespace ConcurrentActorProcessingModel.ActorSystem.Messages
{
    public class ProccessIncomingMessage
    {

        public ProccessIncomingMessage(int incommingMessageId,
            int messageType, int hotelId)
        {
            IncommingMessageId = incommingMessageId;
            MessageType = messageType;
            HotelId = hotelId;
        }
        public ProccessIncomingMessage( 
           int messageType, int hotelId)
        {
             MessageType = messageType;
            HotelId = hotelId;
        }

        public ProccessIncomingMessage(IncomingMessage incomingMessage,long queuedMessageId ,int messageType, int hotelId)
        {
            IncomingMessage = incomingMessage;
            QueuedMessageId = queuedMessageId;
            MessageType = messageType;
            HotelId = hotelId;
        }
        public IncomingMessage IncomingMessage { get; private set; }
        public long QueuedMessageId { get; set; }

        public int IncommingMessageId { get; private set; }
        public int MessageType { get; private set; }
        public int HotelId { get; private set; }
    }
}