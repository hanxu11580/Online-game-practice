using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace C7_Server
{
    // 0为成功 1为失败

    public class MsgRegister:MsgBase
    {
        public MsgRegister()
        {
            protoName = "MsgRegister";
        }

        public string acc;

        public string pw;

        public int result = 0;
    }

    public class MsgLogin : MsgBase
    {
        public MsgLogin()
        {
            protoName = "MsgLogin";
        }

        public string acc;

        public string pw;

        public int result = 0;
    }

    public class MsgKick : MsgBase
    {
        public MsgKick()
        {
            protoName = "MsgKick";
        }

        public int reson = 0;
    }
}
