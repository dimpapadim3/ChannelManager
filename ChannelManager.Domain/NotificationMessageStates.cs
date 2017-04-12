using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChannelManager.Domain
{
    [Table("NotificationMessageStates")]
    public class NotificationMessageState
    {

        public long QueuedMessageId { get; set; }
        [Key]
        public long Id { get; set; }
        public DateTime TimeEntered { get; set; }

        public bool IsCancellation { get; set; }
        public int StateId { get; set; }


    }
}
