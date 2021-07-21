using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Linq;

public class Client_StickPackage : MonoBehaviour
{
    Socket socket;

    public Button Btn_Connect;
    public Button Btn_Send;
    public InputField sendMessage;
    public Text Txt_Recv;
    //
    private string sendStr;
    private byte[] sendBuff = new byte[1024];

    private string recvStr;
    private byte[] recvBuff = new byte[1024];
    private int recvBuffCount; //接收缓冲区数据长度

    private void Start()
    {
        Btn_Connect.onClick.AddListener(Connect);
        Btn_Send.onClick.AddListener(Send);
    }

    public void Connect()
    {
        Debug.Log("主线程" + System.Threading.Thread.CurrentThread.ManagedThreadId);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect("127.0.0.1", 8888);

        socket.BeginReceive(recvBuff, 0, 1024 - recvBuffCount, 0, RecvCallback, socket);
    }

    private void RecvCallback(IAsyncResult ar)
    {
        try
        {
            Socket sk = (Socket)ar.AsyncState;
            int count = sk.EndReceive(ar);
            recvBuffCount += count;
            OnReceiveByteData();
            sk.BeginReceive(recvBuff, 0, 1024 - recvBuffCount, 0, RecvCallback, sk);
        }
        catch (SocketException se)
        {
            Debug.Log(se);
        }
    }

    private void OnReceiveByteData()
    {
        if (recvBuffCount <= 2)
        { // 小于前缀字节长度
            return;
        }
        // 取出
        Int16 bodyLength = BitConverter.ToInt16(recvBuff, 0);

        if (recvBuffCount < bodyLength + 2)
        { // 接收Buff字节数不够一个消息体
            return;
        }

        string s = Encoding.Default.GetString(recvBuff, 2, bodyLength);

        //更新缓冲区
        int copyStart = bodyLength + 2;
        recvBuffCount -= copyStart;
        Array.Copy(recvBuff, copyStart, recvBuff, 0, recvBuffCount);
        recvStr += "\n" + s;
        OnReceiveByteData();
    }


    public void Send()
    {
        sendStr = sendMessage.text;
        sendBuff = Encoding.Default.GetBytes(sendStr);
        Int16 bodyLen = (Int16)sendBuff.Length;
        byte[] bodyLenByte = BitConverter.GetBytes(bodyLen);
        sendBuff = bodyLenByte.Concat(sendBuff).ToArray();
        socket.Send(sendBuff);
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
