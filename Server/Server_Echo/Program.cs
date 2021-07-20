using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
namespace Server_Echo
{
    public class ClientState
    {
        public Socket Client;
        public byte[] recvBuff = new byte[1024];
    }

    class Program
    {
        static Socket socket_Listen;

        static Dictionary<Socket, ClientState> clientStateDict = new Dictionary<Socket, ClientState>();

        static void Main(string[] args)
        {
            Console.WriteLine("Start Server Echo");

            socket_Listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEnd = new IPEndPoint(ipAdr, 8888);

            socket_Listen.Bind(ipEnd);
            socket_Listen.Listen(0);

            socket_Listen.BeginAccept(AcceptCallback, socket_Listen);

            // 这句必须有，否则命令行会直接关闭
            Console.ReadLine();
        }

        static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket sk = ar.AsyncState as Socket;
                Socket clientSocket = sk.EndAccept(ar);
                ClientState clState = new ClientState();
                clState.Client = clientSocket;
                clientStateDict[clientSocket] = clState;
                Console.WriteLine("Client 加入成功，端口号：" + (clientSocket.RemoteEndPoint as IPEndPoint).Port);
                clState.Client.BeginReceive(clState.recvBuff, 0, 1024, 0, RecvCallback, clState);
                sk.BeginAccept(AcceptCallback, sk);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void RecvCallback(IAsyncResult ar)
        {
            try
            {
                ClientState clS = ar.AsyncState as ClientState;
                Socket client = clS.Client;
                int count = client.EndReceive(ar);

                if (count == 0)
                {
                    client.Close();
                    clientStateDict.Remove(client);
                    Console.WriteLine($"客户端Socket关闭");
                    return;
                }
                string recvStr = Encoding.Default.GetString(clS.recvBuff, 0, count);
                //Console.WriteLine($"接收Message：{recvStr}");
                int client_port = (client.RemoteEndPoint as IPEndPoint).Port;
                string sendStr = $"端口-{client_port}：{recvStr}";
                byte[] sendBuff = Encoding.Default.GetBytes(sendStr);
                foreach (ClientState cls in clientStateDict.Values)
                {
                    cls.Client.Send(sendBuff);
                }
                clS.Client.BeginReceive(clS.recvBuff, 0, 1024, 0, RecvCallback, clS);


            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
