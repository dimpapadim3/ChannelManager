using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ChannelManager.Domain;
using ChannelManagerService;
using ChannelManagerService.DataAccess;
using ConcurrentActorProcessingModel.ActorSystem;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConsoleApplication1
{


    class TestConsoleApplication
    {
        public static Room GenerateRoom( )
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
        static void Main(string[] args)
        {
            TestCancelationAfterFirstMessageArrivedUseCase();
        }

   
        private static void TestCancelationAfterFirstMessageArrivedUseCase()
        {
            //EmptyDb();

            //var room = GenerateRoom();

            ActorSystemReference.ActorSystem.ActorOf(Props.Create(() => new ProcccesManagmentActor()));

          //  IChannelManagerService service = new ChannelManagerService.ChannelManagerService();

            //this should fail overlaping dates to existing reservation 
            //service.ProcessIncomingMessage(new IncomingMessage
            //{
            //    ChannelName="Booking.com",
            //    RoomId = room.Id,
            //    MessageType = IncomingChannelMessageType.Reserve,
            //    StartDate = new DateTime(2016, 1, 24),
            //    EndDate = new DateTime(2016, 1, 26),
            //});
            ////this should  fail overlaping dates to existing reservation  
            //service.ProcessIncomingMessage(new IncomingMessage
            //{
            //    ChannelName = "PMS",
            //    RoomId = room.Id,
            //    MessageType = IncomingChannelMessageType.Reserve,
            //    StartDate = new DateTime(2016, 1, 25),
            //    EndDate = new DateTime(2016, 1, 29),
            //});

            Console.Read();
        }
        //static void Main(string[] args)
        //{
        //    EmptyDb();
        //    IActorRef messageCoordinatorActor = MessageCoordinationActor.Instance;

        //    var tasks = new List<Task<ProccessIncomingMessageResponse>>();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        var newIncomingMessage = new ProccessIncomingMessage(
        //     incommingMessageId: i,
        //     messageType: 1,
        //     hotelId: i
        //     );
        //        //Console.WriteLine($"new message send to ChannelManager");

        //        Task<ProccessIncomingMessageResponse> proccessTask =
        //            messageCoordinatorActor.Ask<ProccessIncomingMessageResponse>(newIncomingMessage);

        //        tasks.Add(proccessTask);
        //    }

        //    Task.WaitAll(tasks.ToArray());
        //    tasks.ForEach(t =>
        //    {
        //        Console.WriteLine($"message with id  {t.Result.IncomingMessage.IncommingMessageId }" +
        //                          $" queued as new message with id in queue {t.Result.QueuedMessageId}");

        //    });

        //    Console.Read();
        //}
    }
}
