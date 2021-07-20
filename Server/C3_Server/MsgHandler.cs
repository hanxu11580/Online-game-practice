using System;
using System.Collections.Generic;
using System.Text;

namespace C3_Server
{
    class MsgHandler
    {
        public static void MsgEnter(ClientState cls, string msg)
        {
            string[] splits = msg.Split(',');
            string desc = splits[0];
            float x = float.Parse(splits[1]);
            float y = float.Parse(splits[2]);
            float z = float.Parse(splits[3]);
            float eularY = float.Parse(splits[4]);

            cls.hp = 100;
            cls.x = x;
            cls.y = y;
            cls.z = z;
            cls.eulY = eularY;
            string sendStr = "Enter|" + msg;
            foreach (ClientState clS in MainClass.clientStateDict.Values)
            {
                MainClass.Send(clS, sendStr);
            }


        }

        public static void MsgMove(ClientState cls, string msg)
        {

        }

        public static void MsgLeave(ClientState cls, string msg)
        {

        }
    }
}
