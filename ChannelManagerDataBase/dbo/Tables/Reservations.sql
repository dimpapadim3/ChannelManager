CREATE TABLE [dbo].[Reservations] (
    [RoomId]          INT  NULL,
    [ReservationType] INT  NULL,
    [Id]              INT  IDENTITY (1, 1) NOT NULL,
    [LengthOfStay]    INT  NULL,
    [StartDate]       DATE NULL,
    [EndDate]         DATE NULL,
    CONSTRAINT [PK__Reservat__3214EC0756E97250] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Reservations_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id])
);

