using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConcurrentActorProcessingModel.ActorSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ConcurrentActorProcessingModel.ActorSystem.Messages;

namespace ConcurrentActorProcessingModel.ActorSystem.Tests
{
    [TestClass()]
    public class MessageCoordinatorActorTests
    {
        [TestMethod()]
        public void ProccessIncomingMessageTest()
        {
          //  IActorRef messageCoordinatorActor = ProcccesManagmentActor.Instance;

            //messageCoordinatorActor.Ask<ProccessIncomingMessage>(
            //    new ProccessIncomingMessage(
            //        incommingMessageId: 5,
            //         messageType: 1,
            //         hotelId: 2));

        }
    }
}