using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    public  static class PlayerDataHelper
    {
        public static bool IsAccountExist(this IMongoCollection<DB_Player> collection, string id)
        {
            DB_Player player = collection.Query(p => p.id == id);
            if (player != null)
            {
                return true;
            }
            Console.WriteLine($"id:{id} not exist");
            return false;
        }

        public static bool Register(this IMongoCollection<DB_Player> collection, string id, string pw)
        {
            if (collection.IsAccountExist(id))
            {
                Console.WriteLine("Register fail, id exist");
                return false;
            }

            collection.InsertOne(new DB_Player()
            {
                id = id,
                pw = pw,
                data = new PlayerData()
            });
            Console.WriteLine("Register success");
            return true;
        }

        public static bool CheckPassword(this IMongoCollection<DB_Player> collection, string id, string pw)
        {
            DB_Player dB_Player = collection.Query(p => p.id == id);
            if (dB_Player == null) return false;

            if(dB_Player.id == id && dB_Player.pw == pw)
            {
                return true;
            }
            return false;
        }

        public static PlayerData GetPlayerData(this IMongoCollection<DB_Player> collection, string id)
        {
            DB_Player dB_Player = collection.Query(p => p.id == id);
            if(dB_Player != null)
            {
                return dB_Player.data;
            }
            return null;
        }

        public static bool UpdatePlayerData(this IMongoCollection<DB_Player> collection, string id, PlayerData newData)
        {
            DB_Player dB_Player = collection.Query(p => p.id == id);
            if(dB_Player != null)
            {
                dB_Player.data = newData;
                collection.ReplaceOne(p => p.id == id, dB_Player);
                return true;
            }
            return false;
        }
    }
}
