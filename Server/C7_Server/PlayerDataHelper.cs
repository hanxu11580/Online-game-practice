using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    public  static class PlayerDataHelper
    {
        public static IMongoCollection<DB_Player> DB_Player_Collection;


        public static bool IsAccountExist(string acc)
        {
            DB_Player player = DB_Player_Collection.Query(p => p.acc == acc);
            if (player != null)
            {
                return true;
            }
            Console.WriteLine($"id:{acc} not exist");
            return false;
        }

        public static bool Register(string id, string pw)
        {
            if (IsAccountExist(id))
            {
                Console.WriteLine("Register fail, id exist");
                return false;
            }

            DB_Player_Collection.InsertOne(new DB_Player()
            {
                acc = id,
                pw = pw,
                data = new PlayerData()
            });
            Console.WriteLine("Register success");
            return true;
        }

        public static bool CheckPassword(string id, string pw)
        {
            DB_Player dB_Player = DB_Player_Collection.Query(p => p.acc == id);
            if (dB_Player == null) return false;

            if(dB_Player.acc == id && dB_Player.pw == pw)
            {
                return true;
            }
            return false;
        }

        public static PlayerData GetPlayerData(string id)
        {
            DB_Player dB_Player = DB_Player_Collection.Query(p => p.acc == id);
            if(dB_Player != null)
            {
                return dB_Player.data;
            }
            return null;
        }

        public static bool UpdatePlayerData(string id, PlayerData newData)
        {
            DB_Player dB_Player = DB_Player_Collection.Query(p => p.acc == id);
            if(dB_Player != null)
            {
                dB_Player.data = newData;
                DB_Player_Collection.ReplaceOne(p => p.acc == id, dB_Player);
                return true;
            }
            return false;
        }
    }
}
