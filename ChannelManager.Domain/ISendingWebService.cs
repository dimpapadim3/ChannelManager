using System;

namespace ChannelManager.Domain
{
    public interface ISendingWebService< TReceivedNewIncomingMessage>
    {
        void Activate();
        void SendCancellationMessage(object message);
        event EventHandler<TReceivedNewIncomingMessage> OnReceivedNewIncomingMessage;
    }
}