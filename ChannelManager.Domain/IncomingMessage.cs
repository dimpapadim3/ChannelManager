using System;

namespace ChannelManager.Domain
{
    public enum IncomingChannelMessageType
    {
        Reserve = 1, 
    }

    public class IncomingMessage
    { 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int HotelId { get; set; }
        public int RoomId { get; set; }
        public IncomingChannelMessageType MessageType { get; set; }
        public string ChannelName { get; set; }
    }
}