using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

public class EchoClient : MonoBehaviour
{
    Socket socket;

    public Button Btn_Connect;
    public Button Btn_Send;
    public InputField sendMessage;
    public Text Txt_Recv;
    //
    private byte[] sendBuff = new byte[1024];
    private byte[] recvBuff = new byte[1024];
    private string sendStr;
    private string recvStr;

    private void Start()
    {
        Btn_Connect.onClick.AddListener(Connect);
        Btn_Send.onClick.AddListener(Send);
    }

    public void Connect()
    {
        Debug.Log("主线程" + System.Threading.Thread.CurrentThread.ManagedThreadId);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // 同步
        // socket.Connect("127.0.0.1", 8888);
        // 异步
        socket.BeginConnect("127.0.0.1", 8888, ConnectCallback, socket);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket sk = (Socket)ar.AsyncState;
            sk.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            sk.BeginReceive(recvBuff, 0, 1024, 0, RecvCallback, sk);

        }
        catch (SocketException se)
        {
            // 将会打印 由于目标计算机积极拒绝，无法连接。
            Debug.Log(se);
        }
    }

    private void RecvCallback(IAsyncResult ar)
    {
        try
        {
            Socket sk = (Socket)ar.AsyncState;
            int count = sk.EndReceive(ar);
            recvStr +="\n" + Encoding.Default.GetString(recvBuff, 0, count);
            Debug.Log("当前线程"+System.Threading.Thread.CurrentThread.ManagedThreadId);
            //Txt_Recv.text = recvStr;
            sk.BeginReceive(recvBuff, 0, 1024, 0, RecvCallback, sk);
        }
        catch(SocketException se)
        {
            Debug.Log(se);
        }
    }


    public void Send()
    {
        sendStr = sendMessage.text;
        sendBuff = Encoding.Default.GetBytes(sendStr);
        socket.BeginSend(sendBuff, 0, sendBuff.Length, 0, SendCallback, socket);
        Debug.Log("发送成功");
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket sk = ar.AsyncState as Socket;
            int count = socket.EndSend(ar);
            Debug.Log("成功发送" + count + "个字节"); 
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    private void OnDestroy()
    {
        socket?.Close();
    }

    private void Update()
    {
        Txt_Recv.text = recvStr;
    }


}
