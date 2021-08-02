using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    class EventHandler
    {
        public static void OnDisconnect(ClientState state)
        {
            Console.WriteLine("Close");
        }

        public static void OnTimer()
        {
            CheckPing();
        }

        public static void CheckPing()
        {
            long timeNow = NetManager.GetTimeStamp();
            foreach (ClientState cs in NetManager.clientDict.Values)
            {
                if(timeNow - cs.lastPingTime >= NetManager.pingInterval * 4)
                {
                    Console.WriteLine("Timeout Close " + cs.socket.RemoteEndPoint.ToString());
                    NetManager.Close(cs);
                }
            }
        }

    }
}
