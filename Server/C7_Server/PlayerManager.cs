using System;
using System.Collections.Generic;
using System.Text;

namespace C7_Server
{
    public class PlayerManager
    {
        static Dictionary<string, Player> playerDicts = new Dictionary<string, Player>();

        public static bool IsOnline(string id)
        {
            return playerDicts.ContainsKey(id);
        }

        public static Player GetPlayer(string id)
        {
            return playerDicts[id];
        }

        public static void AddPlayer(string id, Player player)
        {
            playerDicts.Add(id, player);
        }

        public static void RemovePlayer(string id)
        {
            playerDicts.Remove(id);
        }
    }
}
