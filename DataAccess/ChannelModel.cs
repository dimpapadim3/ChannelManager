using System.Data.Entity;
using ChannelManager.Domain;

namespace ChannelManagerService.DataAccess
{
    public partial class ChannelModel : DbContext
    {
        public ChannelModel()
            : base("name=ChannelModel")
        {
        }

        public virtual DbSet<ChannelsMessageQueueMessage> ChannelsMessageQueueMessage { get; set; }
        public virtual DbSet<Hotel> Hotels { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Reservation> Reservetions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
