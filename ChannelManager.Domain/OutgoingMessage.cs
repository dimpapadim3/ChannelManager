namespace ChannelManager.Domain
{
    public enum OutgoingChannelMessageType
    {
        Reserve = 1,
        Cancell = 2,

    }
    public class OutgoingMessage
    {
        public int HotelID { get; set; }
        public OutgoingChannelMessageType MessageType { get; set; }
    }
}