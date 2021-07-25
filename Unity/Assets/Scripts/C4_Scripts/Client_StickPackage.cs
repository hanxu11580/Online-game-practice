using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
        if(Length < 8)
        {
            if(Length > 0)
            {
                Array.Copy(bytes, readIdx, bytes, 0, Length);
            }
            writeIdx = Length; //顺序不能变
            readIdx = 0;
        }
    }

    public int Write(byte[] wirteBytes, int offset, int count)
    {
        if(Remain < count)
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
