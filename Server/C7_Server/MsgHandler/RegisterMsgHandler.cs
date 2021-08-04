using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    public partial class MsgHandler
    {
        public static void MsgRegister(ClientState cs, MsgBase msgBase)
        {
            MsgRegister mr = msgBase as MsgRegister;
            string id = mr.acc;
            string pw = mr.pw;
            
            if(PlayerDataHelper.Register(id, pw))
            {
                mr.result = 0;
            }
            else
            {
                mr.result = 1;
            }
            NetManager.Send(cs, mr);
        }

        public static void MsgLogin(ClientState cs, MsgBase msgBase)
        {
            MsgLogin ml = msgBase as MsgLogin;
            if(!PlayerDataHelper.CheckPassword(ml.acc, ml.pw))
            {
                ml.result = 1;
                NetManager.Send(cs, ml);
                return;
            }

            // 当前客户端重复登陆
            if(cs.player != null)
            { // 重复登陆
                ml.result = 1;
                NetManager.Send(cs, ml);
                return;
            }

            // 其他客户端登陆这个账号
            if (PlayerManager.IsOnline(ml.acc))
            {
                Player other = PlayerManager.GetPlayer(ml.acc);
                MsgKick msg = new MsgKick();
                msg.reson = 0;
                other.Send(msg);
                NetManager.Close(cs);
            }

            PlayerData data = PlayerDataHelper.GetPlayerData(ml.acc);
            if(data == null)
            {
                ml.result = 1;
                NetManager.Send(cs, ml);
                return;
            }

            Player player = new Player(cs)
            {
                acc = ml.acc,
                playerData = data,
            };

            PlayerManager.AddPlayer(ml.acc, player);
            cs.player = player;
            ml.result = 0;
            player.Send(ml);
        }
    }
}
