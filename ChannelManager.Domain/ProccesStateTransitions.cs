using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelManager.Domain
{
    [Table("ProccesStateTransitionsHistory")]
    public partial class ProccesStateTransitions
    {
        public long QueuedMessageId { get; set; }
        [Key]
        public long Id { get; set; }
        public DateTime TimeTransitioned { get; set; }

        public bool IsInFaultState { get; set; }
        public int StateId { get; set; }
        public string ErrorMessage { get; set; }





    }
}