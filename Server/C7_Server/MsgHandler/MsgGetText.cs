using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    public partial class MsgHandler
    {
        public static void MsgGetText(ClientState cs, MsgBase msgBase)
        {
            MsgGetText msg = msgBase as MsgGetText;
            Player player = cs.player;
            if(player != null)
            {
                msg.text = player.playerData.text;
                player.Send(msg);
            }
        }

        public static void MsgSaveText(ClientState cs, MsgBase msgBase)
        {
            MsgSaveText msg = msgBase as MsgSaveText;
            Player player = cs.player;
            if (player != null)
            {
                msg.result = 0;
                player.playerData.text = msg.text;
                PlayerDataHelper.UpdatePlayerData(player.acc, player.playerData);
                player.Send(msg);
            }
        }
    }
}
