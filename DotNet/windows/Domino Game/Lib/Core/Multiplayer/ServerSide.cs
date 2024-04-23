using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Domino_Game.Lib.Core.Multiplayer
{
#warning "Todo: Not used"

    public class ServerSide
    {
        public TcpListener server { get; set; }
        public IPAddress ip
        {
            get
            {
                return server.Server.LocalEndPoint is IPEndPoint endPoint ? endPoint.Address : IPAddress.Any;
            }
        }

        public int port { get; set; }

        public List<TcpClient> clients { get; set; }


        public ServerSide()
        {
            server = new TcpListener(IPAddress.Any, port);
            clients = new List<TcpClient>();
        }

        public void Start()
        {
            server?.Start();
        }

        public void Stop()
        {
            server?.Stop();
        }

        public void AcceptClient()
        {
            TcpClient client = server?.AcceptTcpClient() ?? throw new NullReferenceException();
            clients.Add(client);
        }

        public void SendToAll(string message)
        {
            foreach (var client in clients)
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        public void SendToClient(string message, TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public string ReceiveFromClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = new byte[client.ReceiveBufferSize];
            int bytesRead = stream.Read(data, 0, client.ReceiveBufferSize);
            return Encoding.UTF8.GetString(data, 0, bytesRead);
        }

        public void CloseClient(TcpClient client)
        {
            client.Close();
            clients.Remove(client);
        }
    }
}
