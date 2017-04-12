CREATE TABLE [dbo].[NotificationMessageStates] (
    [Id]              BIGINT        IDENTITY (1, 1) NOT NULL,
    [QueuedMessageId] BIGINT        NOT NULL,
    [StateId]         INT           NOT NULL,
    [IsCancellation]  BIT           NULL,
    [TimeEntered]     DATETIME2 (7) NULL,
    CONSTRAINT [PK_NotificationMessageStates] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_NotificationMessageStates_ChannelsMessageQueueMessage] FOREIGN KEY ([QueuedMessageId]) REFERENCES [dbo].[ChannelsMessageQueueMessage] ([MessageId])
);

