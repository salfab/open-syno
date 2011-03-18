using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSyno.Services
{
    public class ReadWriteMemoryStream : MemoryStream
    {
        private readonly object _lockObject = new object();

        public ReadWriteMemoryStream(int size) : base(size)
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_lockObject)
            {
                return base.Read(buffer, offset, count);
            }
        }

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
