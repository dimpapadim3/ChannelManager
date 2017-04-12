using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;

namespace ConcurrentActorProcessingModel.ActorSystem.ProccesStates
{
    public interface IStateContext
    {
        ILoggingAdapter Log { get; }
        State State { get; set; }
        void TransitionToState(State newState);
        IDictionary<string, IActorRef> ResgisteredWebServices { get; set; }
    }
}