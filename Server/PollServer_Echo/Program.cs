using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
namespace PollServer_Echo
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

            // 普通Poll
            //while (true)
            //{
            //    if(socket_Listen.Poll(0, SelectMode.SelectRead))
            //    {
            //        ReadListen(socket_Listen);
            //    }

            //    foreach (ClientState cls in clientStateDict.Values)
            //    {
            //        Socket sk = cls.Client;
            //        if(sk.Poll(0, SelectMode.SelectRead))
            //        {
            //            if (!ReadClient(sk)) break;

            //        }
            //    }

            //    System.Threading.Thread.Sleep(1);
            //}


            // 多路复用Poll
            List<Socket> checkSockets = new List<Socket>();
            while (true)
            {
                checkSockets.Clear();
                checkSockets.Add(socket_Listen);

                foreach (ClientState cls in clientStateDict.Values)
                {
                    checkSockets.Add(cls.Client);
                }

                Socket.Select(checkSockets, null, null, 1000);

                foreach (Socket canReadSocket in checkSockets)
                {
                    if(canReadSocket == socket_Listen)
                    {
                        ReadListen(canReadSocket);
                    }
                    else
                    {
                        ReadClient(canReadSocket);
                    }
                }
            }
        }

        static void ReadListen(Socket listen)
        {
            Socket clientSocket = listen.Accept();
            ClientState clState = new ClientState();
            clState.Client = clientSocket;
            clientStateDict[clientSocket] = clState;
        }

        static bool ReadClient(Socket client)
        {
            ClientState cls = clientStateDict[client];
            int count = 0;
            try
            {
                count = client.Receive(cls.recvBuff);
            }catch(Exception e)
            {
                CloseClientSocket(client);
                Console.WriteLine("Receive SocketException-Socket Closed" + e);
                return false;
            }

            if(count == 0)
            {
                CloseClientSocket(client);
                return false;
            }

            string recvStr = Encoding.Default.GetString(cls.recvBuff, 0, count);
            int client_port = (client.RemoteEndPoint as IPEndPoint).Port;
            string sendStr = $"端口-{client_port}：{recvStr}";
            byte[] sendBuff = Encoding.Default.GetBytes(sendStr);
            foreach (ClientState clS in clientStateDict.Values)
            {
                clS.Client.Send(sendBuff);
            }
            return true;
        }

        static void CloseClientSocket(Socket sk)
        {
            sk.Close();
            clientStateDict.Remove(sk);
        }
    }
}
