using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ChannelManager.Domain;

namespace ChannelManagerService.DataAccess
{
    public class ChannelsUnitOfWork : IDisposable
    {
        private readonly ChannelModel _context = new ChannelModel();
        private GenericRepository<ChannelsMessageQueueMessage> _queueMessagesRepository;
        private GenericRepository<Hotel> _hotelRepository;
        private GenericRepository<Room> _roomRepository;
        private GenericRepository<Reservation> _reservetionRepository;

        public GenericRepository<ChannelsMessageQueueMessage> QueueMessageRepository
        {
            get
            {

                if (this._queueMessagesRepository == null)
                {
                    this._queueMessagesRepository = new GenericRepository<ChannelsMessageQueueMessage>(_context);
                }
                return _queueMessagesRepository;
            }
        }
        public GenericRepository<Hotel> HotelsRepository
        {
            get
            {

                if (this._hotelRepository == null)
                {
                    this._hotelRepository = new GenericRepository<Hotel>(_context);
                }
                return _hotelRepository;
            }
        }
        public GenericRepository<Room> RoomsRepository
        {
            get
            {

                if (this._roomRepository == null)
                {
                    this._roomRepository = new GenericRepository<Room>(_context);
                }
                return _roomRepository;
            }
        }
        public GenericRepository<Reservation> ReservationRepository
        {
            get
            {

                if (this._reservetionRepository == null)
                {
                    this._reservetionRepository = new GenericRepository<Reservation>(_context);
                }
                return _reservetionRepository;
            }
        }

        public int Save()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Save exception {exception}");
                throw;
            }

        }
        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}