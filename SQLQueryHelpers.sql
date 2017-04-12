--delete from ChannelsMessageQueueMessage
--delete from Hotels
--delete from Rooms
--delete from reservations
select * from Hotels
select * from Rooms
select * from ChannelsMessageQueueMessage
select * from Reservations
select NotificationState,ChannelName from ChannelsMessageQueueMessage