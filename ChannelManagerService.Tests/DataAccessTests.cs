using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChannelManagerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChannelManager.Domain;
using ChannelManagerService.DataAccess;

namespace ChannelManagerService.Tests
{
    [TestClass()]
    public class DataAccessTests
    {
        public void EmptyDb()
        {
            var model = new ChannelModel();
            model.Database.ExecuteSqlCommand("Delete from ChannelsMessageQueueMessage");
            model.Database.ExecuteSqlCommand("Delete from Reservations");
            model.Database.ExecuteSqlCommand("Delete from Rooms");

            model.Database.ExecuteSqlCommand("Delete from Hotels");
        }
        [TestMethod()]
        public void ProcessIncomingMessageTest_ShouldInsertIncomingMessageToQueue()
        {
            EmptyDb();
            ChannelsUnitOfWork channelsUnitOfWork = new ChannelsUnitOfWork();
            var hotel = new Hotel();
            channelsUnitOfWork.HotelsRepository.Insert(hotel);
            channelsUnitOfWork.Save();

            var entity = new Room()
            {
                HotelId = hotel.Id
            };

            channelsUnitOfWork.RoomsRepository.Insert(entity);
            channelsUnitOfWork.Save();

            channelsUnitOfWork.ReservationRepository.Insert(new Reservation()
            {
                RoomId = entity.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2)
            });
            channelsUnitOfWork.Save();

        }


    }
}