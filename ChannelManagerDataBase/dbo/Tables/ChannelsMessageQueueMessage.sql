CREATE TABLE [dbo].[ChannelsMessageQueueMessage] (
    [HotelID]      INT            NOT NULL,
    [MessageId]    BIGINT         IDENTITY (1, 1) NOT NULL,
    [MessageType]  INT            NULL,
    [IsProcceced]  BIT            NULL,
    [Message]      NVARCHAR (MAX) NULL,
    [TimeReceived] DATETIME2 (7)  NULL,
    [RoomId]       INT            NULL,
    [ChannelName] NVARCHAR(50) NULL, 
    [ProccessingState] INT NULL, 
    [NotificationState] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_ChannelsMessageQueueMessage] PRIMARY KEY CLUSTERED ([MessageId] ASC)
);



