using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteMemoryStream"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public ReadWriteMemoryStream(int size) : base(size)
        {
            
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
            lock (_lockObject)
            {
                return base.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from. </param><param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing from. </param><param name="count">The maximum number of bytes to write. </param><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception><exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite"/>.-or- The current position is closer than <paramref name="count"/> bytes to the end of the stream, and the capacity cannot be modified. </exception><exception cref="T:System.ArgumentException"><paramref name="offset"/> subtracted from the buffer length is less than <paramref name="count"/>. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> are negative. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.ObjectDisposedException">The current stream instance is closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_lockObject)
            {
                var oldPosition = base.Position;
                base.Position = base.Length;
                base.Write(buffer, offset, count);
                base.Position = oldPosition;
            }
        }
    }
}
