using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelManager.Domain
{
    [Table("ChannelsMessageQueueMessage")]
    public partial class ChannelsMessageQueueMessage
    {
        public string NotificationState { get; set; }
        public int HotelID { get; set; }
        public int RoomId { get; set; }
        public int MessageType { get; set; }
        [Key]
        public long MessageId { get; set; }
        public DateTime TimeReceived { get; set; }
         
        public bool IsProcceced { get; set; }
        public string Message { get; set; }
        public string ChannelName { get; set; }
        public int ProccessingState { get; set; }
    }
}
