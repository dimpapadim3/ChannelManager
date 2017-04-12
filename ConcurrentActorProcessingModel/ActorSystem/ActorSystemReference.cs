using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Ninject;
using ChannelManager.Domain;
using Ninject;


namespace ConcurrentActorProcessingModel.ActorSystem
{
    public static class ActorSystemReference
    {
        public static Akka.Actor.ActorSystem ActorSystem { get; private set; }

        static ActorSystemReference()
        {
            CreateActorSystem();
        } 
        private static void CreateActorSystem()
        {
            ActorSystem = Akka.Actor.ActorSystem.Create("ReactiveChannelsActorSystem");

            var container = new StandardKernel();
            container.Bind<ISendingWebService<IncomingMessage>>().To<BookingComWebService.ChannelWebService>();
            container.Bind<BookingComCommunicationActor>().To(typeof(BookingComCommunicationActor));
         
            IDependencyResolver resolver = new NinjectDependencyResolver(container, ActorSystem);


        }
    }
}
