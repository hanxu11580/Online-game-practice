using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Linq;

public class ByteArray
{
    public byte[] bytes;
    public int readIdx;
    public int writeIdx;

    public int Length => writeIdx - readIdx;

    public ByteArray(byte[] bytes)
    {
        this.bytes = bytes;
        readIdx = 0;
        writeIdx = bytes.Length;
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
    private int recvBuffCount; //���ջ��������ݳ���

    private Queue<ByteArray> writeQue = new Queue<ByteArray>();


    private void Start()
    {
        Btn_Connect.onClick.AddListener(Connect);
        Btn_Send.onClick.AddListener(Send);
    }

    public void Connect()
    {
        Debug.Log("���߳�" + System.Threading.Thread.CurrentThread.ManagedThreadId);

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
            Debug.Log($"���յ�:{count},��ʱrecvBuff��{recvBuffCount}");
            OnReceiveByteData();
            System.Threading.Thread.Sleep(10000);
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
        { // С��ǰ׺�ֽڳ���
            return;
        }
        // ȡ��
        // Int16 bodyLength = BitConverter.ToInt16(recvBuff, 0);
        Int16 bodyLength = (short)((recvBuff[1] << 8) | (recvBuff[0]));

        if (recvBuffCount < bodyLength + 2)
        { // ����Buff�ֽ�������һ����Ϣ��
            return;
        }

        string s = Encoding.Default.GetString(recvBuff, 2, bodyLength);
        //���»�����
        int copyStart = bodyLength + 2;
        recvBuffCount -= copyStart;
        Array.Copy(recvBuff, copyStart, recvBuff, 0, recvBuffCount);
        recvStr += "\n" + s;
        Debug.Log($"�����������ַ���:{s}-�����Ϣ�����СΪ{copyStart},��ʱrecvBuff��{recvBuffCount}");
        OnReceiveByteData();
    }


    public void Send()
    {
        //��װ
        sendStr = sendMessage.text;
        sendBuff = Encoding.Default.GetBytes(sendStr);
        Int16 bodyLen = (Int16)sendBuff.Length;
        byte[] bodyLenByte = BitConverter.GetBytes(bodyLen);
        if (!BitConverter.IsLittleEndian)
        {
            bodyLenByte.Reverse();
        }
        sendBuff = bodyLenByte.Concat(sendBuff).ToArray();
        //����
        ByteArray ba = new ByteArray(sendBuff);
        int countQue = 0;
        lock (writeQue)
        {
            writeQue.Enqueue(ba);
            countQue = writeQue.Count;
        }

        if (writeQue.Count == 1)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.Length, 0, SendCallback, socket);
        }
        
    }

    private void SendCallback(IAsyncResult ar)
    {
        Socket sk = ar.AsyncState as Socket;
        int sendCount = sk.EndSend(ar);
        ByteArray firstBa;
        lock (writeQue)
        {
            firstBa = writeQue.First();
        }

        firstBa.readIdx += sendCount;
        if(firstBa.Length == 0)
        { // ba��û��ʣ��
            lock (writeQue)
            {
                writeQue.Dequeue();
                firstBa = writeQue.First();
            }
        }

        if(firstBa != null)
        { //�����ǵڶ���Ҳ�����ǵ�һ��ʣ���
            sk.BeginSend(firstBa.bytes, firstBa.readIdx, firstBa.Length, 0, SendCallback, sk);
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
