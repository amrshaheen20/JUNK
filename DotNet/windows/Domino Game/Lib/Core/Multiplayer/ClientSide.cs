using System.Net.Sockets;
using System.Text;

namespace Domino_Game.Lib.Core.Multiplayer
{

#warning "Todo: Not used"
    public class ClientSide
    {
        public TcpClient ClientConnection { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        public ClientSide(string ServerIP, int ServerPort)
        {
            IP = ServerIP;
            Port = ServerPort;
            ClientConnection = new TcpClient();
        }

        public bool IsConnected
        {
            get
            {
                return ClientConnection.Connected;
            }
        }

        public void Connect()
        {
            ClientConnection?.Connect(IP, Port);
        }

        public void Disconnect()
        {
            ClientConnection?.Close();
        }


        public void Send(string message)
        {
            if (!IsConnected)
            {
                throw new Exception("Client is not connected to the server");
            }

            NetworkStream stream = ClientConnection.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public string Receive()
        {
            if (!IsConnected)
            {
                throw new Exception("Client is not connected to the server");
            }

            NetworkStream stream = ClientConnection.GetStream();
            byte[] data = new byte[ClientConnection.ReceiveBufferSize];
            int bytesRead = stream.Read(data, 0, ClientConnection.ReceiveBufferSize);
            return Encoding.UTF8.GetString(data, 0, bytesRead);
        }

    }
}
