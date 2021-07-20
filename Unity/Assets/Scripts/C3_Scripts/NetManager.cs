using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
public static class NetManager
{
    // 消息格式 消息名称|IP地址,消息体
    //          Move    |127.0.0.1,1234,10,0,8


    static Socket socket;

    static byte[] recvBuff = new byte[1024];

    public delegate void MessageListener(string str);

    // key:消息名字 value 消息回调
    static Dictionary<string, MessageListener> listeners = new Dictionary<string, MessageListener>();

    // 消息列表
    static List<string> messageList = new List<string>();

    public static void AddListener(string msgName, MessageListener listener)
    {
        listeners.Add(msgName, listener);
    }

    public static bool SocketIsNotValid => socket == null || !socket.Connected;

    public static string GetDesc()
    {
        if (SocketIsNotValid) return string.Empty;
        return socket.LocalEndPoint.ToString();
    }

    public static void Connect(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ip, port);
        socket.BeginReceive(recvBuff, 0, 1024, 0, RecvCallback, socket);
    }

    private static void RecvCallback(IAsyncResult ar)
    {
        try
        {
            Socket sk = ar.AsyncState as Socket;
            int count = sk.EndReceive(ar);
            string msgStr = Encoding.Default.GetString(recvBuff, 0, count);
            messageList.Add(msgStr);
            sk.BeginReceive(recvBuff, 0, 1024, 0, RecvCallback, sk);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    public static void Send(string sendStr)
    {
        if (SocketIsNotValid) return;
        byte[] sendBuff = Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBuff);
    }

    public static void Update()
    {
        if (messageList.Count > 0)
        {
            string firstMsg = messageList[0];
            messageList.RemoveAt(0);

            string[] splitMsg = firstMsg.Split('|');
            string msgName = splitMsg[0];
            string otherMsg = splitMsg[1];
            if(listeners.TryGetValue(msgName, out MessageListener messageListener))
            {
                messageListener?.Invoke(otherMsg);
            }
        }
    }
}
