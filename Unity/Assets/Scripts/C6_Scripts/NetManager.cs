using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Linq;

namespace C6
{
    #region ByteArray

    public class ByteArray
    {
        const int Default_Size = 1024;

        int initSize = 0;

        public byte[] bytes;

        public int readIdx;
        public int writeIdx;

        private int capacity = 0;

        public int Remain => capacity - writeIdx;

        public int Length => writeIdx - readIdx;

        public ByteArray(int size = Default_Size)
        {
            bytes = new byte[size];
            capacity = size;
            initSize = size;
            readIdx = 0;
            writeIdx = 0;
        }

        public ByteArray(byte[] defaultBytes)
        {
            bytes = defaultBytes;
            capacity = defaultBytes.Length;
            initSize = defaultBytes.Length;
            readIdx = 0;
            writeIdx = defaultBytes.Length;
        }

        // 重新设置大小
        public void ReSize(int size)
        {
            if (size < Length) return; //小于当前长度copy不进去
            if (size < initSize) return;
            int n = 1;
            while (n < size) n *= 2; //每次长度翻倍
            capacity = n;
            byte[] newBytes = new byte[capacity];
            Array.Copy(bytes, readIdx, newBytes, 0, Length);
            bytes = newBytes;
            writeIdx = Length;
            readIdx = 0;
        }

        // 前移动数组,提高可用长度
        public void MoveBytes()
        {
            if (Length < 8)
            {
                if (Length > 0)
                {
                    Array.Copy(bytes, readIdx, bytes, 0, Length);
                }
                writeIdx = Length; //顺序不能变
                readIdx = 0;
            }
        }

        public int Write(byte[] wirteBytes, int offset, int count)
        {
            if (Remain < count)
            {
                // 扩容
                ReSize(Length + count);
            }
            Array.Copy(wirteBytes, offset, bytes, writeIdx, count);
            writeIdx += count;
            return count;
        }


        public int Read(byte[] bs, int offset, int count)
        {
            // 当前长度只有3个，然后他要读8个，所以我只给他3个
            count = Mathf.Min(count, Length);
            Array.Copy(bytes, readIdx, bs, offset, count);
            readIdx += count;
            MoveBytes();
            return count;
        }

        public short ReadInt16()
        {
            if (Length < 2) return 0;
            // 默认小端
            short ret = (short)((bytes[1] << 8) | bytes[0]);
            readIdx += 2;
            MoveBytes();
            return ret;
        }

        public int ReadInt32()
        {
            if (Length < 4) return 0;
            // 默认小端
            int ret = (short)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]);
            readIdx += 4;
            MoveBytes();
            return ret;
        }

        //调试
        public override string ToString()
        {
            return BitConverter.ToString(bytes, readIdx, Length);
        }

        public void Debug()
        {
            string debugStr = $"readIdx({readIdx}),writeIdx({writeIdx}),bytes({BitConverter.ToString(bytes, 0, bytes.Length)})";
            UnityEngine.Debug.Log(debugStr);
        }
    }

    #endregion

    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3
    }


    public static class NetManager
    {
        static Socket socket;

        static ByteArray readBuff;

        static Queue<ByteArray> writeQue;

        public delegate void EventListener(string err);
        // 网络事件
        static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

        public delegate void MsgListener(MsgBase msg);
        // 消息事件
        static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

        static bool isConnecting = false;

        static bool isClosing = false;

        // 接收

        static List<MsgBase> msgLists = new List<MsgBase>();

        static int msgListCount = 0;

        readonly static int MAX_MESSAGE_THROW = 10;

        // 心跳协议

        public static bool IsUsePing = true;

        public static int pingInterval = 30;

        static float lastPingTime;

        static float lastPongTime;


        static void InitState()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            readBuff = new ByteArray();
            writeQue = new Queue<ByteArray>();
            msgLists = new List<MsgBase>();
            msgListCount = 0;
            isConnecting = false;
            isClosing = false;
            lastPingTime = Time.time;
            lastPongTime = Time.time;

            if (!msgListeners.ContainsKey("MsgPong"))
            {
                AddMsgListener("MsgPong", OnMsgPong);
            }

        }

        private static void OnMsgPong(MsgBase msg)
        {
            Debug.Log("Receive Pong");
            lastPongTime = Time.time;
        }


        #region 网络事件功能
        public static void AddEventListener(NetEvent netEvent, EventListener listener)
        {
            if (eventListeners.TryGetValue(netEvent, out EventListener el))
            {
                el += listener;
            }
            else
            {
                eventListeners[netEvent] = listener;
            }
        }

        public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
        {
            if (eventListeners.TryGetValue(netEvent, out EventListener el))
            {
                el -= listener;
                if (el == null)
                {
                    eventListeners.Remove(netEvent);
                }
            }
        }

        public static void ThrowEvent(NetEvent netEvent, string err)
        {
            if (eventListeners.TryGetValue(netEvent, out EventListener el))
            {
                el(err);
            }
        }


        #endregion

        #region 消息事件

        public static void AddMsgListener(string msgName, MsgListener ml)
        {
            if(msgListeners.TryGetValue(msgName, out MsgListener msgListener))
            {
                msgListener += ml;
            }
            else
            {
                msgListeners[msgName] = ml;
            }
        }

        public static void RemoveMsgListener(string msgName, MsgListener ml)
        {
            if (msgListeners.TryGetValue(msgName, out MsgListener msgListener))
            {
                msgListener -= ml;
                if(msgListener == null)
                {
                    msgListeners.Remove(msgName);
                }
            }
        }

        public static void ThrowMsgListener(string msgName, MsgBase msg)
        {
            if (msgListeners.TryGetValue(msgName, out MsgListener msgListener))
            {
                msgListener(msg);
            }
        }

        #endregion

        #region 连接

        public static void Connect(string ip, int port)
        {
            if(socket != null && isConnecting)
            {
                Debug.LogError("已连接 Already Connected");
                return;
            }

            if (isConnecting)
            {
                Debug.LogError("正在连接 IsConnecting");
                return;
            }

            InitState();
            socket.NoDelay = true; // 不会积攒小消息
            isConnecting = true;
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket sk = ar.AsyncState as Socket;
                sk.EndConnect(ar);
                Debug.Log("连接成功 Connect Succ");
                ThrowEvent(NetEvent.ConnectSucc, string.Empty);
                isConnecting = false;

                // 接收数据
                sk.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0, ReceiveCallback, sk);
            }
            catch(SocketException se)
            {
                string errStr = se.ToString();
                Debug.Log("连接失败 Connect Failed" + errStr);
                ThrowEvent(NetEvent.ConnectFail, errStr);
                isConnecting = false;
            }
        }

        #endregion

        #region 关闭
        public static void Close()
        {
            Debug.Log("Socket Closed");

            if(socket == null || !socket.Connected)
            {
                return;
            }

            if (isClosing)
            {
                return;
            }

            if(writeQue.Count > 0)
            {
                isClosing = true;
            }
            else
            {
                socket.Close();
                ThrowEvent(NetEvent.Close, "");
            }
        }


        #endregion

        #region 发送
        public static void Send(MsgBase msg)
        {
            if (socket == null || !socket.Connected) return;
            if (isConnecting) return;
            if (isClosing) return;
            byte[] protoNameBytes = MsgBase.EncodeProtoName(msg);
            byte[] msgBytes = MsgBase.Encode(msg);
            int len = protoNameBytes.Length + msgBytes.Length;
            byte[] sendBytes = new byte[len+2]; //发送信息还有头部信息
            sendBytes[0] = (byte)(len >> 0);
            sendBytes[1] = (byte)(len >> 8);
            // 组装协议名称
            Array.Copy(protoNameBytes, 0, sendBytes, 2, protoNameBytes.Length);
            // 组装协议
            Array.Copy(msgBytes, 0, sendBytes, protoNameBytes.Length + 2, msgBytes.Length);
            ByteArray ba = new ByteArray(sendBytes);
            int writeQueCount = 0;
            lock (writeQue)
            {
                writeQue.Enqueue(ba);
                writeQueCount = writeQue.Count;
            }

            if(writeQueCount == 1)
            {
                socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
            }

        }

        static void SendCallback(IAsyncResult ar)
        {
            Socket sk = ar.AsyncState as Socket;
            if (sk == null || !sk.Connected) return;
            int sendCount = sk.EndSend(ar);
            ByteArray firstBa;
            lock (writeQue)
            {
                firstBa = writeQue.First();
            }
            firstBa.readIdx += sendCount;
            if(firstBa.Length == 0)
            { //当前已发完 It has been send out
                lock (writeQue)
                {
                    writeQue.Dequeue();
                    firstBa = writeQue.Dequeue();
                }
            }
            // 此时可能firstBa未发完，或者队列下一个需要发送的
            if(firstBa != null)
            {
                sk.BeginSend(firstBa.bytes, firstBa.readIdx, firstBa.Length, 0, SendCallback, sk);
            }else if (isClosing)
            {// writeQue.Count == 0
                sk.Close();
            }
        }

        #endregion

        #region 接收
        static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket sk = ar.AsyncState as Socket;
                int recvCount = sk.EndReceive(ar);
                if (recvCount == 0)
                {//接收到FIN
                    Close();
                    return;
                }

                readBuff.writeIdx += recvCount;
                OnReceiveData();
                if(readBuff.Remain < 8)
                { // 由于不知道下次数据量多少,剩余不足直接扩容
                    readBuff.MoveBytes();
                    readBuff.ReSize(readBuff.Length * 2);
                }

                sk.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0,ReceiveCallback, sk);
            }
            catch(SocketException se)
            {
                Debug.Log("Socket Receive Failed" + se.ToString());
            }
        }

        // 2个功能
        // 1.接收头部信息 判断是否可用处理消息
        // 2.解析并分发消息
        static void OnReceiveData()
        {
            if (readBuff.Length <= 2) return;
            int readIdx = readBuff.readIdx;
            byte[] bytes = readBuff.bytes;
            short msgLen = (short)(bytes[readIdx] | (bytes[readIdx+1] << 8));
            if (readBuff.Length < msgLen + 2) return;
            // 够一条消息了
            readBuff.readIdx += 2;
            // 解析协议名称
            string protoName = MsgBase.DecodeProtoName(readBuff.bytes, readBuff.readIdx, out int protoNameLen);
            if (string.IsNullOrEmpty(protoName))
            {
                Debug.Log("ProtoName Decode Failed protoName is Null");
                return;
            }
            readBuff.readIdx += protoNameLen; //这里已经加上了协议名的2个字节长度的信息
            // 解析消息体
            int msgBodyLen = msgLen - protoNameLen;
            // 这里因为我加了命名空间，GetType必须要有命名空间.
            // 这里如果不加因为不在主线程，所以还不会报错
            MsgBase msgBase = MsgBase.Decode($"C6.{protoName}", readBuff.bytes, readBuff.readIdx, msgBodyLen);
            readBuff.readIdx += msgBodyLen;
            readBuff.MoveBytes();
            lock (msgLists)
            {
                msgLists.Add(msgBase);
            }
            msgListCount++;
            if(readBuff.Length > 2)
            {
                OnReceiveData();
            }
        }


        #endregion

        static void MsgUdpate()
        {
            if (msgListCount == 0) return;
            for (int i = 0; i < MAX_MESSAGE_THROW; i++)
            {
                MsgBase msgBase = null;
                lock (msgLists)
                {
                    if(msgLists.Count > 0)
                    {
                        msgBase = msgLists[0];
                        msgLists.RemoveAt(0);
                        msgListCount--;
                    }
                }
                if(msgBase != null)
                {
                    ThrowMsgListener(msgBase.protoName, msgBase);
                }
                else
                {
                    break;
                }
            }
        }

        // 是否启用
        // 根据lastPingTime 间隔发送ping
        // 根据lastPongTime 是否关闭
        static void PingpongUpdate()
        {
            if (!IsUsePing) return;
            
            // 间隔发送
            if (Time.time - lastPingTime >= pingInterval)
            {
                MsgPing msgPing = new MsgPing();
                Send(msgPing);
                lastPingTime = Time.time;
            }

            // 判断是否有回应
            if(Time.time - lastPongTime > pingInterval * 4)
            {
                Debug.Log($"关闭时间戳之差:{Time.time - lastPongTime}");
                Close();
            }
        }

        public static void Update()
        {
            MsgUdpate();
            PingpongUpdate();
        }
    }

}