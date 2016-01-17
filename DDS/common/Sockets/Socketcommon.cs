using System;
using System.Net.Sockets;

namespace OMS.common.Sockets
{
    public enum TSocketDataMode
    {
        sdmOmsLine,
        sdmFixLine,
        sdmQueueLine,
        sdmOG,
        sdmBinary
    }

    public class SocketStatusEventArgs : EventArgs
    {
        protected bool connected;

        public SocketStatusEventArgs(bool connected)
        {
            this.connected = connected;
        }

        public bool Connected { get { return connected; } }
    }

    public class SocketReceiveEventArgs : EventArgs
    {
        protected string message;

        public SocketReceiveEventArgs(string message)
        {
            this.message = message;
        }

        public string Message { get { return message; } }
    }

    public class SocketServerReceiveEventArgs : SocketReceiveEventArgs
    {
        protected TSocketClient sock;

        public SocketServerReceiveEventArgs(TSocketClient sock, string message)
            : base(message)
        {
            this.sock = sock;
        }

        public TSocketClient Socket { get { return sock; } }
    }

    public class BinaryReceiveEventArgs : EventArgs
    {
        protected byte[] buffer;
        protected int offset;
        protected int size;

        public BinaryReceiveEventArgs(byte[] buffer, int offset, int size)
        {
            this.buffer = buffer;
            this.offset = offset;
            this.size = size;
        }

        public byte[] Buffer { get { return buffer; } }

        public int Offset { get { return offset; } }

        public int Size { get { return size; } }
    }

    public class SocketBroadcastEventArgs : SocketReceiveEventArgs
    {
        public SocketBroadcastEventArgs(string message)
            : base(message)
        { }
    }

    public class SocketClientConnectEventArgs : SocketStatusEventArgs
    {
        protected string clientID;

        public SocketClientConnectEventArgs(bool connected, string clientID)
            : base(connected)
        {
            this.clientID = clientID;
        }

        public string ClientID { get { return clientID; } }
    }

    public class SocketErrorEventArgs : EventArgs
    {
        protected Exception lastError;

        public SocketErrorEventArgs(Exception error)
        {
            this.lastError = error;
        }

        public Exception LastError { get { return lastError; } }
    }

    public class DataBufferEventArgs : EventArgs
    {
        protected byte[] buffer;
        protected int len;

        public DataBufferEventArgs(byte[] buffer, int len)
        {
            this.buffer = buffer;
            this.len = len;
        }

        public byte[] Buffer { get { return buffer; } }

        public int Length { get { return len; } }
    }
}