using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ninject;
using OpemSyno.Contracts.Services;
using OpenSyno.Helpers;
using OpenSyno;

namespace OpenSyno.Services
{
    /// <summary>
    /// A memory stream with the ability to write data at the end of it while reading at an arbitrary position.
    /// </summary>
    /// <remarks>
    /// Only the <see cref="Read"/> and <see cref="Write"/> method were implemented.
    /// This stream can be typically used to start using the first chunks of data of a file before it is completely downloaded, when it is located on a slow link. Example : Start playing an mp3 file before it is completely downloaded.
    /// </remarks>
    public class ReadWriteMemoryStream : MemoryStream
    {
        /// <summary>
        /// An instance member which will be used as a token to enter exclusive blocks of code within the class.
        /// </summary>
        private readonly object _lockObject = new object();

        private ILogService _logService;
        private DateTime _lastFailedRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteMemoryStream"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public ReadWriteMemoryStream(int size) : base(size)
        {
            _readTimeout = 30000;
            _logService = IoC.Container.Get<ILogService>();
        }

        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to <paramref name="buffer"/>.
        /// </summary>
        /// <returns>
        /// The total number of bytes written into the buffer. This can be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached before any bytes are read.
        /// </returns>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the characters read from the current stream. </param><param name="offset">The byte offset in <paramref name="buffer"/> at which to begin reading. </param><param name="count">The maximum number of bytes to read. </param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception><exception cref="T:System.ArgumentException"><paramref name="offset"/> subtracted from the buffer length is less than <paramref name="count"/>. </exception><exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;


                try
                {

                    _lastFailedRead = DateTime.MaxValue;

                    do
                    {
                        lock (_lockObject)
                        {
                            read = base.Read(buffer, offset, count);
                            if (read == 0)
                            {
                                // logService.Trace("RWMS.Read : stream reading is starved : last failed @ " + _lastFailedRead);
                            }

                        }
                        if (read == 0 && _lastFailedRead == DateTime.MaxValue)
                        {
                            _lastFailedRead = DateTime.Now;
                        }
                    } while (read == 0 && this.Position < this.Length && (_lastFailedRead == DateTime.MaxValue ||_lastFailedRead.AddMilliseconds(ReadTimeout) > DateTime.Now));

                    if (read == 0 && this.Position < this.Length)
                    {
                        _logService.Trace("Connection lost, data could not be read. Position : " + Position + "Length : " + Length );
                    }
                }
                
                catch (Exception e)
                {
                    _logService.Trace(string.Format("ReadWriteMemoryStream.Read : Read error : {0} - {1}", e.GetType().FullName, e.Message));
                    // TODO : maybe we should swallow the error and raise an event so that we can relay the problem to the UI.
                    // ... this or handle the exceptions in the Mp3MediaStreamSource + MediaParser dll so the apps don't crash with unhandled exceptions.
                    throw;
                }
                       
            _lastFailedRead = DateTime.MaxValue;            
            return read;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            AsyncCallback internalBeginReadCallback = ar =>
                                                          {                                                          
                                                              var read = this.EndRead(ar);
                                                              ThreadPool.QueueUserWorkItem(o =>
                                                              {
                                                                  while (read == 0 && this.Position < this.Length && (_lastFailedRead == DateTime.MaxValue || _lastFailedRead.AddMilliseconds(ReadTimeout) < DateTime.Now))
                                                                  {
                                                                      read = base.Read(buffer, offset, count);
                                                                      if (read == 0 && _lastFailedRead == DateTime.MaxValue)
                                                                      {
                                                                          _lastFailedRead = DateTime.Now;
                                                                      }

                                                                  }
                                                                  if (read == 0)
                                                                  {
                                                                      _logService.Trace("Connection lost, data could not be read.");
                                                                  }

                                                                  callback(ar);
                                                              });

                                                          };
            return base.BeginRead(buffer, offset, count, internalBeginReadCallback, state);
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        private int _readTimeout;
        private bool _isStarving;

        public override int ReadTimeout
        {
            get { return _readTimeout; }
            set { _readTimeout = value; }
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from. </param><param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing from. </param><param name="count">The maximum number of bytes to write. </param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite"/>.-or- The current position is closer than <paramref name="count"/> bytes to the end of the stream, and the capacity cannot be modified. </exception><exception cref="T:System.ArgumentException"><paramref name="offset"/> subtracted from the buffer length is less than <paramref name="count"/>. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> are negative. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {

            if (_isStarving)
            {
                _logService.Trace("ReadWriteMemoryStream.Write : Writing while starving BEFORE LOCK");
            }
            try
            {
                lock (_lockObject)
                {
                    if (_isStarving)
                    {
                        _logService.Trace("ReadWriteMemoryStream.Write : Writing while starving");                        
                    }
                    if (this.CanWrite)
                    {
                        var oldPosition = base.Position;
                        base.Position = base.Length;
                        base.Write(buffer, offset, count);                    
                        base.Position = oldPosition;
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Trace(string.Format("ReadWriteMemoryStream.Write : {0} - {1}", e.GetType().FullName, e.Message));
                throw;
            }
           
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void Close()
        {
            lock (_lockObject)
            {
                Debug.WriteLine("Closing stream : " + this.GetHashCode());
                base.Close();
            }
        }
    }
}
