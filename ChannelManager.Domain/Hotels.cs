using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelManager.Domain
{
    [Table("Hotels")]
    public partial class Hotel
    {
        [Key]
        public int Id { get; set; }
       
    }
}
