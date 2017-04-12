using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelManager.Domain
{
    [Table("Reservations")]
    public partial class Reservation
    {
        [Key]
        public int Id { get; set; }
        public int ReservationType { get; set; }
        
        public int RoomId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

         
    }
}
