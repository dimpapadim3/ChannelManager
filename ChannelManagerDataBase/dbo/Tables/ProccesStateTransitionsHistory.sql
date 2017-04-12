CREATE TABLE [dbo].[ProccesStateTransitionsHistory] (
    [Id]               BIGINT        IDENTITY (1, 1) NOT NULL,
    [StateId]          INT           NOT NULL,
    [IsInFaultState]   BIT           NOT NULL,
    [TimeTransitioned] DATETIME2 (7) NOT NULL,
    [QueuedMessageId]  BIGINT        NULL,
    CONSTRAINT [PK_ProccesStateTransitionsHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProccesStateTransitionsHistory_ChannelsMessageQueueMessage] FOREIGN KEY ([QueuedMessageId]) REFERENCES [dbo].[ChannelsMessageQueueMessage] ([MessageId])
);

