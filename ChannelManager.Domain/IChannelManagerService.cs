namespace ChannelManager.Domain
{
    public   interface IChannelManagerService
    {
         void  ProcessIncomingMessage(IncomingMessage incomingMessage);
    
    }
}