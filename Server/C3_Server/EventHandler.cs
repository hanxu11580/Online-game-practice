using System;
using System.Collections.Generic;
using System.Text;

namespace C3_Server
{
    class EventHandler
    {
        public static void OnDisconnect(ClientState cls)
        {
            string desc = cls.Client.RemoteEndPoint.ToString();
            string sendStr = "Leave|" + desc + ",";
            foreach (ClientState clS in MainClass.clientStateDict.Values)
            {
                MainClass.Send(clS, sendStr);
            }
        }
    }
}
