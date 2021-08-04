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

        // �������ô�С
        public void ReSize(int size)
        {
            if (size < Length) return; //С�ڵ�ǰ����copy����ȥ
            if (size < initSize) return;
            int n = 1;
            while (n < size) n *= 2; //ÿ�γ��ȷ���
            capacity = n;
            byte[] newBytes = new byte[capacity];
            Array.Copy(bytes, readIdx, newBytes, 0, Length);
            bytes = newBytes;
            writeIdx = Length;
            readIdx = 0;
        }

        // ǰ�ƶ�����,��߿��ó���
        public void MoveBytes()
        {
            if (Length < 8)
            {
                if (Length > 0)
                {
                    Array.Copy(bytes, readIdx, bytes, 0, Length);
                }
                writeIdx = Length; //˳���ܱ�
                readIdx = 0;
            }
        }

        public int Write(byte[] wirteBytes, int offset, int count)
        {
            if (Remain < count)
            {
                // ����
                ReSize(Length + count);
            }
            Array.Copy(wirteBytes, offset, bytes, writeIdx, count);
            writeIdx += count;
            return count;
        }


        public int Read(byte[] bs, int offset, int count)
        {
            // ��ǰ����ֻ��3����Ȼ����Ҫ��8����������ֻ����3��
            count = Mathf.Min(count, Length);
            Array.Copy(bytes, readIdx, bs, offset, count);
            readIdx += count;
            MoveBytes();
            return count;
        }

        public short ReadInt16()
        {
            if (Length < 2) return 0;
            // Ĭ��С��
            short ret = (short)((bytes[1] << 8) | bytes[0]);
            readIdx += 2;
            MoveBytes();
            return ret;
        }

        public int ReadInt32()
        {
            if (Length < 4) return 0;
            // Ĭ��С��
            int ret = (short)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]);
            readIdx += 4;
            MoveBytes();
            return ret;
        }

        //����
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
        // �����¼�
        static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

        public delegate void MsgListener(MsgBase msg);
        // ��Ϣ�¼�
        static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

        static bool isConnecting = false;

        static bool isClosing = false;

        // ����

        static List<MsgBase> msgLists = new List<MsgBase>();

        static int msgListCount = 0;

        readonly static int MAX_MESSAGE_THROW = 10;

        // ����Э��

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


        #region �����¼�����
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

        #region ��Ϣ�¼�

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

        #region ����

        public static void Connect(string ip, int port)
        {
            if(socket != null && isConnecting)
            {
                Debug.LogError("������ Already Connected");
                return;
            }

            if (isConnecting)
            {
                Debug.LogError("�������� IsConnecting");
                return;
            }

            InitState();
            socket.NoDelay = true; // �������С��Ϣ
            isConnecting = true;
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket sk = ar.AsyncState as Socket;
                sk.EndConnect(ar);
                Debug.Log("���ӳɹ� Connect Succ");
                ThrowEvent(NetEvent.ConnectSucc, string.Empty);
                isConnecting = false;

                // ��������
                sk.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0, ReceiveCallback, sk);
            }
            catch(SocketException se)
            {
                string errStr = se.ToString();
                Debug.Log("����ʧ�� Connect Failed" + errStr);
                ThrowEvent(NetEvent.ConnectFail, errStr);
                isConnecting = false;
            }
        }

        #endregion

        #region �ر�
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

        #region ����
        public static void Send(MsgBase msg)
        {
            if (socket == null || !socket.Connected) return;
            if (isConnecting) return;
            if (isClosing) return;
            byte[] protoNameBytes = MsgBase.EncodeProtoName(msg);
            byte[] msgBytes = MsgBase.Encode(msg);
            int len = protoNameBytes.Length + msgBytes.Length;
            byte[] sendBytes = new byte[len+2]; //������Ϣ����ͷ����Ϣ
            sendBytes[0] = (byte)(len >> 0);
            sendBytes[1] = (byte)(len >> 8);
            // ��װЭ������
            Array.Copy(protoNameBytes, 0, sendBytes, 2, protoNameBytes.Length);
            // ��װЭ��
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
            { //��ǰ�ѷ��� It has been send out
                lock (writeQue)
                {
                    writeQue.Dequeue();
                    firstBa = writeQue.Dequeue();
                }
            }
            // ��ʱ����firstBaδ���꣬���߶�����һ����Ҫ���͵�
            if(firstBa != null)
            {
                sk.BeginSend(firstBa.bytes, firstBa.readIdx, firstBa.Length, 0, SendCallback, sk);
            }else if (isClosing)
            {// writeQue.Count == 0
                sk.Close();
            }
        }

        #endregion

        #region ����
        static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket sk = ar.AsyncState as Socket;
                int recvCount = sk.EndReceive(ar);
                if (recvCount == 0)
                {//���յ�FIN
                    Close();
                    return;
                }

                readBuff.writeIdx += recvCount;
                OnReceiveData();
                if(readBuff.Remain < 8)
                { // ���ڲ�֪���´�����������,ʣ�಻��ֱ������
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

        // 2������
        // 1.����ͷ����Ϣ �ж��Ƿ���ô�����Ϣ
        // 2.�������ַ���Ϣ
        static void OnReceiveData()
        {
            if (readBuff.Length <= 2) return;
            int readIdx = readBuff.readIdx;
            byte[] bytes = readBuff.bytes;
            short msgLen = (short)(bytes[readIdx] | (bytes[readIdx+1] << 8));
            if (readBuff.Length < msgLen + 2) return;
            // ��һ����Ϣ��
            readBuff.readIdx += 2;
            // ����Э������
            string protoName = MsgBase.DecodeProtoName(readBuff.bytes, readBuff.readIdx, out int protoNameLen);
            if (string.IsNullOrEmpty(protoName))
            {
                Debug.Log("ProtoName Decode Failed protoName is Null");
                return;
            }
            readBuff.readIdx += protoNameLen; //�����Ѿ�������Э������2���ֽڳ��ȵ���Ϣ
            // ������Ϣ��
            int msgBodyLen = msgLen - protoNameLen;
            // ������Ϊ�Ҽ��������ռ䣬GetType����Ҫ�������ռ�.
            // �������������Ϊ�������̣߳����Ի����ᱨ��
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

        // �Ƿ�����
        // ����lastPingTime �������ping
        // ����lastPongTime �Ƿ�ر�
        static void PingpongUpdate()
        {
            if (!IsUsePing) return;
            
            // �������
            if (Time.time - lastPingTime >= pingInterval)
            {
                MsgPing msgPing = new MsgPing();
                Send(msgPing);
                lastPingTime = Time.time;
            }

            // �ж��Ƿ��л�Ӧ
            if(Time.time - lastPongTime > pingInterval * 4)
            {
                Debug.Log($"�ر�ʱ���֮��:{Time.time - lastPongTime}");
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