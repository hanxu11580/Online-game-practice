using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

namespace C6
{
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
            writeIdx = 0;
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

        static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

        static bool isConnecting = false;

        static bool isClosing = false;

        #region 事件功能
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

        static void InitState()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            readBuff = new ByteArray();
            writeQue = new Queue<ByteArray>();
            isConnecting = false;
            isClosing = false;
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
    }

}