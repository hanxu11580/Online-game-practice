using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace C6
{

    public class ProtoTest : MonoBehaviour
    {
        public byte[] Encode(IExtensible msgBase)
        {
            using (var memory = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memory, msgBase);
                return memory.ToArray();
            }
        }

        public IExtensible Decode(string protoName, byte[] bytes, int offset, int count)
        {
            using(var memory = new MemoryStream(bytes, offset, count))
            {
                Type type = Type.GetType(protoName);
                IExtensible ie = (IExtensible)ProtoBuf.Serializer.NonGeneric.Deserialize(type, memory);
                return ie;
            }
        }

        private void Start()
        {
            proto.MsgMove msg = new proto.MsgMove()
            {
                x = 1,
                y = 2,
                z = 3
            };

            byte[] bytes = Encode(msg);

            Debug.Log("Proto转化"+ bytes.Length+"个字节..." + System.BitConverter.ToString(bytes));
            proto.MsgMove deMsg = (proto.MsgMove)Decode("proto.MsgMove", bytes, 0, bytes.Length);
            Debug.Log(deMsg.x);

            MsgMove msg1 = new MsgMove()
            {
                x = 1,
                y = 2,
                z = 3
            };

            bytes = MsgBase.Encode(msg1);
            Debug.Log("Json转化"+ bytes.Length + "个字节..." + System.BitConverter.ToString(bytes));
            C6.MsgMove deMsg1 = (C6.MsgMove)MsgBase.Decode("C6.MsgMove", bytes, 0, bytes.Length);
            Debug.Log(deMsg1.z);
        }
    }
}