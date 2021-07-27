using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace C6
{
    /// <summary>
    /// 协议基类
    /// </summary>
    public class MsgBase
    {
        public string protoName = "";

        #region 协议名编码/解码
        public static byte[] EncodeProtoName(MsgBase msgBase)
        {
            byte[] bytesName = Encoding.UTF8.GetBytes(msgBase.protoName);
            short nameLen = (short)bytesName.Length;
            byte[] bytes = new byte[bytesName.Length + 2];
            // 默认小端
            bytes[0] = (byte)(nameLen >> 0);
            bytes[1] = (byte)(nameLen >> 8);
            Array.Copy(bytesName, 0, bytes, 2, nameLen);
            return bytes;
        }


        #endregion

        public static byte[] Encode(MsgBase msgBase)
        {
            string s = JsonUtility.ToJson(msgBase);
            return Encoding.UTF8.GetBytes(s);
        }

        public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
        {
            string s = Encoding.UTF8.GetString(bytes, offset, count);
            return (MsgBase)JsonUtility.FromJson(s, Type.GetType(protoName));
        }

    }

    public class MsgMove : MsgBase
    {
        public MsgMove()
        {
            protoName = "MsgMove";
        }

        public int x = 0;
        public int y = 0;
        public int z = 0;

    }

    public class MsgAttack : MsgBase
    {
        public MsgAttack()
        {
            protoName = "MsgAttack";
        }

        public string desc = "127.0.0.1:6453";

    }
}