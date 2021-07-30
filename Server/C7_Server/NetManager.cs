using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Linq;
using System.Net;
using System.Reflection;

namespace C7_Server
{
    class NetManager
    {
        public static Socket listenfd;

        public static Dictionary<Socket, ClientState> clientDict = new Dictionary<Socket, ClientState>();

        static List<Socket> checkSockets = new List<Socket>();

        public static void StartLoop(int listenPort)
        {
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse("0.0.0.0");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, listenPort);
            listenfd.Bind(ipEp);
            listenfd.Listen(0);
            Console.WriteLine("服务器启动...");

            while (true)
            {
                ResetCheckSocket();
                Socket.Select(checkSockets, null, null, 0);
                for (int i = checkSockets.Count - 1; i >= 0; i--)
                {
                    Socket sk = checkSockets[i];
                    if (sk == listenfd)
                    { // 连接请求
                        ReadListenfd(sk);
                    }
                    else
                    {
                        ReadClinetfd(sk);
                    }
                }

            }
        }

        static void ResetCheckSocket()
        {
            checkSockets.Clear();
            checkSockets.Add(listenfd);
            foreach (ClientState cs in clientDict.Values)
            {
                checkSockets.Add(cs.socket);
            }
        }

        static void ReadListenfd(Socket socket)
        {
            try
            {
                Socket client = socket.Accept();
                Console.WriteLine("Accept " + client.RemoteEndPoint.ToString());
                ClientState cState = ClientState.Create(client);
                clientDict.Add(client, cState);
            }
            catch(SocketException se)
            {
                Console.WriteLine("Accept Failed:" + se.ToString());
            }
        }
        #region 接收数据

        static void ReadClinetfd(Socket clientfd)
        {
            ClientState cState = clientDict[clientfd];
            ByteArray readBuff = cState.readBuff;
            int receiveCount = 0;
            if(readBuff.Remain <= 0)
            {
                OnReceiveData(cState);
                readBuff.MoveBytes();
            }
            if(readBuff.Remain <= 0)
            {
                Console.WriteLine("Receive Failed msgLength > BuffCapacity");
                Close(cState);
                return;
            }

            try
            {
                receiveCount = clientfd.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0);
            }
            catch(SocketException se)
            {
                Console.WriteLine("Receive Eexception " + se.ToString());
                Close(cState);
                return;
            }

            if(receiveCount <= 0)
            { // 客户端主动关闭
                Console.WriteLine("Socket Closed" + cState.socket.RemoteEndPoint.ToString());
                Close(cState);
                return;
            }

            readBuff.writeIdx += receiveCount;
            OnReceiveData(cState);
            readBuff.MoveBytes();
        }

        static void OnReceiveData(ClientState state)
        {
            ByteArray readBuff = state.readBuff;
            byte[] bytes = readBuff.bytes;
            int readIdx = readBuff.readIdx;
            if (readBuff.Length <= 2) return;
            short msgLen = (short)((bytes[readIdx + 1] << 8) | (bytes[readIdx]);
            if (readBuff.Length < msgLen + 2) return;
            readBuff.readIdx += 2;
            string protoName = MsgBase.DecodeProtoName(readBuff.bytes, readBuff.readIdx, out int protoNameCount);
            if (string.IsNullOrEmpty(protoName))
            {
                Console.WriteLine("DecodeProtoName Failed");
                Close(state);
            }
            readBuff.readIdx += protoNameCount;
            //
            int msgBodyCount = msgLen - protoNameCount;
            MsgBase msgBase = MsgBase.Decode($"C7_Server.{protoName}", readBuff.bytes, readBuff.readIdx, msgBodyCount);
            readBuff.readIdx += msgBodyCount;
            readBuff.MoveBytes();
            //
            MethodInfo mi = typeof(C7_Server.MsgHandler).GetMethod($"On{protoName}");
            object[] args = { state, msgBase };
            mi?.Invoke(null, args);
            Console.WriteLine("Receive Msg: " + protoName);
            if(readBuff.Length > 2)
            {
                OnReceiveData(state);
            }
        }

        #endregion

        #region 关闭

        public static void Close(ClientState state)
        {
            MethodInfo mi = typeof(C7_Server.EventHandler).GetMethod("OnDisconnect");
            object[] args = { state };
            mi.Invoke(null, args);

            state.socket.Close();
            clientDict.Remove(state.socket);
        }

        #endregion
    }
}
