using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelManager.Domain
{
    [Table("Rooms")]
    public partial class Room
    {
        [Key]
        public int Id { get; set; }
        public int  HotelId { get; set; }
    }
}
