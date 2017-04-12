using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ChannelManager.Domain;

namespace ChannelManagerService.Tests
{
    //internal class MockIncomingMessageGateway : IIcomingMessageGateway
    //{
    //    public event EventHandler<IncomingMessage> IncomingMessageReceived;

    //    private Task HandleTimer()
    //    {
    //        return Task.Run(() =>
    //        {
    //            IncomingMessageReceived?.Invoke(this, new IncomingMessage
    //            {
    //                HotelId = 43,
    //                MessageType = IncomingChannelMessageType.Reserve
    //            });
    //        });
    //    }

    //    public List<Task> StartSendMessages()
    //    {
    //        List<Task> tasks = new List<Task>();
    //        tasks.Add(Task.Run(() =>
    //        {
    //            IncomingMessageReceived?.Invoke(this, new IncomingMessage
    //            {

    //                HotelId = 43,
    //                MessageType = IncomingChannelMessageType.Reserve,
    //                StartDate = DateTime.Now,
    //                EndDate = DateTime.Now.AddDays(2)
    //            });
    //        }));

    //        tasks.Add(Task.Run(() =>
    //        {
    //            IncomingMessageReceived?.Invoke(this, new IncomingMessage
    //            {
    //                HotelId = 43,
    //                MessageType = IncomingChannelMessageType.Reserve,
    //                StartDate = DateTime.Now,
    //                EndDate = DateTime.Now.AddDays(2)
    //            });
    //        }));
    //        return tasks;
    //    }
    //    //public List<Task> StartSendMessages()
    //    //{
    //    //    List<Task> tasks = new List<Task>();
    //    //    tasks.Add(Task.Run(() =>
    //    //   {
    //    //       IncomingMessageReceived?.Invoke(this, new IncomingMessage
    //    //       {

    //    //           HotelId = 43,
    //    //           MessageType = IncomingChannelMessageType.Reserve,
    //    //           StartDate = DateTime.Now,
    //    //           EndDate = DateTime.Now.AddDays(2)
    //    //       });
    //    //   }));

    //    //    tasks.Add(Task.Run(() =>
    //    //    {
    //    //        IncomingMessageReceived?.Invoke(this, new IncomingMessage
    //    //        {
    //    //            HotelId = 43,
    //    //            MessageType = IncomingChannelMessageType.Reserve,
    //    //            StartDate = DateTime.Now,
    //    //            EndDate = DateTime.Now.AddDays(2)
    //    //        });
    //    //    }));
    //    //    return tasks; 
    //    //}
    //    public MockIncomingMessageGateway()
    //    {

    //    }

    //}
}
