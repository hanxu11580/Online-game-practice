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

        public static string DecodeProtoName(byte[] bytes, int offset, out int count)
        {
            count = 0;
            if (offset + 2 > bytes.Length) return string.Empty;
            // 默认小端
            short nameLen = (short)(bytes[offset] | (bytes[offset + 1] << 8));
            if (nameLen <= 0) return string.Empty;
            if (offset + 2 + nameLen > bytes.Length) return string.Empty;
            count = nameLen + 2;
            // offset + 2 去了个头
            string decodeProtoName = Encoding.UTF8.GetString(bytes, offset + 2, nameLen);
            return decodeProtoName;
        }

        #endregion

        #region 协议本体编码

        public static byte[] Encode(MsgBase msgBase)
        {
            string s = JsonUtility.ToJson(msgBase);
            return Encoding.UTF8.GetBytes(s);
        }

        public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
        {
            string s = Encoding.UTF8.GetString(bytes, offset, count);
            MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, Type.GetType(protoName));
            return msgBase;

            #endregion
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