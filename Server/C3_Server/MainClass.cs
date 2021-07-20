using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
namespace C3_Server
{
    public class ClientState
    {
        public Socket Client;
        public byte[] recvBuff = new byte[1024];
        public int hp = -100;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulY = 0;
    }

    class MainClass
    {
        static Socket socket_Listen;

        public static Dictionary<Socket, ClientState> clientStateDict = new Dictionary<Socket, ClientState>();

        static void Main(string[] args)
        {
            Console.WriteLine("Start Server Echo");

            socket_Listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEnd = new IPEndPoint(ipAdr, 8888);

            socket_Listen.Bind(ipEnd);
            socket_Listen.Listen(0);

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
                    if (canReadSocket == socket_Listen)
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
            }
            catch (Exception e)
            {
                CloseClientSocket(cls);
                Console.WriteLine("Receive SocketException-Socket Closed" + e);

                return false;
            }

            if (count <= 0)
            {
                CloseClientSocket(cls);
                return false;
            }

            string recvStr = Encoding.Default.GetString(cls.recvBuff, 0, count);
            Console.WriteLine(recvStr);
            string[] splitStrs = recvStr.Split('|');
            string msgName = splitStrs[0];
            string msgBody = splitStrs[1];
            string fullName = "Msg" + msgName;
            MethodInfo mi = typeof(MsgHandler).GetMethod(fullName);
            object[] arg = { cls, msgBody };
            mi.Invoke(null, arg);
            //byte[] sendBuff = Encoding.Default.GetBytes(sendStr);
            //foreach (ClientState clS in clientStateDict.Values)
            //{
            //    clS.Client.Send(sendBuff);
            //}
            return true;
        }

        static void CloseClientSocket(ClientState cls)
        {
            MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
            object[] ob = { cls };
            mei.Invoke(null, ob);

            cls.Client.Close();
            clientStateDict.Remove(cls.Client);
        }

        public static void Send(ClientState cls, string sendStr)
        {
            byte[] sendBuff = Encoding.Default.GetBytes(sendStr);
            cls.Client.Send(sendBuff);
        }
    }
}
