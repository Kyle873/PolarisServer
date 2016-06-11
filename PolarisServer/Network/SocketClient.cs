using System;
using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketClient
    {
        public delegate void ConnectionLostDelegate();
        public delegate void DataReceivedDelegate(byte[] data, int size);
        public event ConnectionLostDelegate ConnectionLost;
        public event DataReceivedDelegate DataReceived;

        public TcpClient Socket { get; private set; }
        public bool NeedsToWrite { get { return (writePosition > 0); } }

        private readonly byte[] readBuffer, writeBuffer;
        private readonly SocketServer server;
        private int writePosition = 0;

        public SocketClient(SocketServer server, TcpClient socket)
        {
            this.server = server;
            Socket = socket;

            readBuffer = new byte[1024 * 16];
            writeBuffer = new byte[1024 * 1024]; // too high? too low? not sure
        }

        public bool OnReadable()
        {
            try
            {
                var read = Socket.Client.Receive(readBuffer);
                if (read == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    server.NotifyConnectionClosed(this);

                    return false;
                }

                DataReceived(readBuffer, read);

                return true;
            }
            catch (SocketException)
            {
                ConnectionLost();
                server.NotifyConnectionClosed(this);

                return false;
            }
        }

        public bool OnWritable()
        {
            try
            {
                var write = Socket.Client.Send(writeBuffer, 0, writePosition, SocketFlags.None);
                if (write == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    server.NotifyConnectionClosed(this);

                    return false;
                }

                Array.Copy(writeBuffer, write, writeBuffer, 0, writePosition - write);
                writePosition -= write;

                return true;
            }
            catch (SocketException)
            {
                ConnectionLost();
                server.NotifyConnectionClosed(this);

                return false;
            }
        }

        public void Write(byte[] blob)
        {
            if ((writePosition + blob.Length) > writeBuffer.Length)
            {
                // Buffer exceeded!
                throw new Exception("Too much data in write queue");
            }

            Array.Copy(blob, 0, writeBuffer, writePosition, blob.Length);
            writePosition += blob.Length;
        }

        public void Close()
        {
            ConnectionLost();
            server.NotifyConnectionClosed(this);
            Socket.Close();
        }
    }
}
