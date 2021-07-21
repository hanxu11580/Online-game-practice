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

        public static void MsgList(ClientState cls, string msg)
        {
            string sendStr = "List|";
            foreach (ClientState clS in MainClass.clientStateDict.Values)
            {
                sendStr += clS.Client.RemoteEndPoint.ToString() + ",";
                sendStr += clS.x.ToString() + ",";
                sendStr += clS.y.ToString() + ",";
                sendStr += clS.z.ToString() + ",";
                sendStr += clS.eulY.ToString() + ",";
                sendStr += clS.hp.ToString() + ",";
            }
            MainClass.Send(cls, sendStr);
        }

        public static void MsgMove(ClientState cls, string msg)
        {
            string[] split = msg.Split(',');
            string desc = split[0];
            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            cls.x = x;
            cls.y = y;
            cls.z = z;
            string sendStr = "Move|" + msg;
            foreach (ClientState clS in MainClass.clientStateDict.Values)
            {
                MainClass.Send(clS, sendStr);
            }
        }

        public static void MsgLeave(ClientState cls, string msg)
        {

        }
    }
}
