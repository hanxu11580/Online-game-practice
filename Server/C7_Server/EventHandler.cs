using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    class EventHandler
    {
        public static void OnDisconnect(ClientState state)
        {
            Console.WriteLine($"{state.socket.RemoteEndPoint} Close");
            if (state.player != null)
            {
                PlayerDataHelper.UpdatePlayerData(state.player.acc, state.player.playerData);
                PlayerManager.RemovePlayer(state.player.acc);
            }

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
                if (cs.lastPingTime == 0) continue; //没有接收到第一个ping
                if(timeNow - cs.lastPingTime >= NetManager.pingInterval * 4)
                {
                    Console.WriteLine("Timeout Close " + cs.socket.RemoteEndPoint.ToString());
                    NetManager.Close(cs);
                }
            }
        }

    }
}
