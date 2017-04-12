CREATE TABLE [dbo].[Rooms] (
    [Id]      INT IDENTITY (1, 1) NOT NULL,
    [HotelId] INT NULL,
    CONSTRAINT [PK__tmp_ms_x__3214EC074DB332A7] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Rooms_Hotels] FOREIGN KEY ([HotelId]) REFERENCES [dbo].[Hotels] ([Id])
);




