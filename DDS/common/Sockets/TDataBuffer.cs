using System;

namespace OMS.common.Sockets
{
    public class TDataBuffer
    {
        protected const int DEFAULTSIZE = 1024;
        protected byte[] buffer;
        protected int readPtr = 0;
        protected int writePtr = 0;

        public TDataBuffer()
            : this(DEFAULTSIZE)
        { }

        public TDataBuffer(int size)
        {
            buffer = new byte[size];
        }

        public int Size { get { return writePtr - readPtr; } }

        public int Read(byte[] dst, int offset, int count)
        {
            int sizeToRead = Math.Min(Size, count);

            Buffer.BlockCopy(buffer, readPtr, dst, offset, sizeToRead);
            readPtr += sizeToRead;

            if (readPtr == writePtr)
            {
                readPtr = writePtr = 0;
            }
            else if (readPtr > (buffer.Length >> 1))
            {
                Buffer.BlockCopy(buffer, readPtr, buffer, 0, Size);
                writePtr = Size;
                readPtr = 0;
            }

            return sizeToRead;
        }

        public int Peek(byte[] dst, int offset, int count)
        {
            int SizeToRead = Math.Min(Size, count);

            Buffer.BlockCopy(buffer, readPtr, dst, offset, SizeToRead);
            return SizeToRead;
        }

        public bool Write(byte[] dst, int offset, int count)
        {
            while (count > (buffer.Length - writePtr)) Expand();

            Buffer.BlockCopy(dst, offset, buffer, writePtr, count);
            writePtr += count;

            return true;
        }

        public void Clear()
        {
            writePtr = readPtr = 0;
        }

        protected void Expand()
        {
            byte[] newbuf = new byte[buffer.Length << 1];
            Buffer.BlockCopy(buffer, readPtr, newbuf, 0, writePtr - readPtr);
            buffer = newbuf;
            writePtr = Size;
            readPtr = 0;
        }
    }
}