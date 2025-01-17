﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{

    public class Player
    {
        public string acc = string.Empty;

        public ClientState clientState;

        public int x;
        public int y;
        public int z;

        public PlayerData playerData;

        public Player(ClientState cs)
        {
            this.clientState = cs;
        }

        public void Send(MsgBase msg)
        {
            NetManager.Send(clientState, msg);
        }
    }

    public class PlayerData
    {
        public int coin = 0;

        public string text = "new text";
    }

    public class DB_Player
    {
        public ObjectId Id { get; set; }

        public string acc;

        public string pw;

        public PlayerData data;
    }
}
