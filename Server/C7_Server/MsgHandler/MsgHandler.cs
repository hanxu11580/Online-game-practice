using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    public partial class MsgHandler
    {
        public static void MsgPing(ClientState cs, MsgBase msgBase)
        {
            Console.WriteLine("处理MsgPing");
            cs.lastPingTime = NetManager.GetTimeStamp();
            MsgPong msgPong = new MsgPong();
            NetManager.Send(cs, msgPong);
        }

        public static void MsgMove(ClientState cs, MsgBase msg)
        {
            MsgMove msgMove = msg as MsgMove;
            Console.WriteLine(msgMove.x);
            msgMove.x++;
            NetManager.Send(cs, msg);
        }
    }
}
