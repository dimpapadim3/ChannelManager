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
    public class ChannelManagerServiceTests
    {

        public static Room GenerateRoom()
        {
            EmptyDb();

            using (ChannelsUnitOfWork channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                var hotel = new Hotel();
                channelsUnitOfWork.HotelsRepository.Insert(hotel);
                channelsUnitOfWork.Save();

                var entity = new Room()
                {
                    HotelId = hotel.Id
                };

                channelsUnitOfWork.RoomsRepository.Insert(entity);
                channelsUnitOfWork.Save();

                return entity;
            }

        }

        public static Reservation GenerateReservation(DateTime startTime, DateTime endTime)
        {
            EmptyDb();

            using (ChannelsUnitOfWork channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                var hotel = new Hotel();
                channelsUnitOfWork.HotelsRepository.Insert(hotel);
                channelsUnitOfWork.Save();

                var entity = new Room()
                {
                    HotelId = hotel.Id
                };

                channelsUnitOfWork.RoomsRepository.Insert(entity);
                channelsUnitOfWork.Save();

                var reservation = new Reservation()
                {
                    RoomId = entity.Id,
                    StartDate = startTime,
                    EndDate = endTime
                };
                channelsUnitOfWork.ReservationRepository.Insert(reservation);
                channelsUnitOfWork.Save();
                return reservation;
            }

        }

        public static void EmptyDb()
        {
            using (var model = new ChannelModel())
            {
                model.Database.ExecuteSqlCommand("Delete from ChannelsMessageQueueMessage");
                model.Database.ExecuteSqlCommand("Delete from Reservations");
                model.Database.ExecuteSqlCommand("Delete from Rooms");

                model.Database.ExecuteSqlCommand("Delete from Hotels");
            }
        }


        [TestMethod()]
        public void ProcessIncomingMessageTest_MockGateway()
        {
            EmptyDb();


            var room = GenerateRoom();

            IChannelManagerService service = new ChannelManagerService();

            service.ProcessIncomingMessage(new IncomingMessage
            {
                ChannelName = "PMS",
                RoomId = room.Id,
                MessageType = IncomingChannelMessageType.Reserve,
                StartDate = new DateTime(2016, 1, 24),
                EndDate = new DateTime(2016, 1, 26),
            });

            using (ChannelsUnitOfWork channelsUnitOfWork = new ChannelsUnitOfWork())
            {
                var messages = channelsUnitOfWork.QueueMessageRepository.Get();
                Assert.IsTrue(messages.Count() == 1);
               // Assert.IsTrue(messages.First().ProccessingState == (int)ProcessingStates.NotifiededChannelsSuccesfully);
            }





        }
    }
}