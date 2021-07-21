using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

public class EchoPollClient : MonoBehaviour
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

    private List<Socket> checkSockets = new List<Socket>();

    private void Start()
    {
        Btn_Connect.onClick.AddListener(Connect);
        Btn_Send.onClick.AddListener(Send);
    }

    public void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 8888);
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
        catch (Exception e)
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
        if (socket == null) return;

        //if(socket.Poll(0, SelectMode.SelectRead))
        //{
        //    int count = socket.Receive(recvBuff);
        //    recvStr = Encoding.Default.GetString(recvBuff, 0, count);
        //    Txt_Recv.text += "\n" + recvStr;
        //}

        checkSockets.Clear();
        checkSockets.Add(socket);
        Socket.Select(checkSockets, null, null, 0);

        foreach (Socket socket in checkSockets)
        {
            int count = socket.Receive(recvBuff);
            recvStr = Encoding.Default.GetString(recvBuff, 0, count);
            Txt_Recv.text += "\n" + recvStr;
        }
    }
}
