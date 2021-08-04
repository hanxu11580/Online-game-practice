using System;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.IO;
using MongoDB.Driver;

namespace C7_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            IMongoDatabase localDatabase = DBManager.Connect("127.0.0.1", 27017, "OnlineTest");
            PlayerDataHelper.DB_Player_Collection = localDatabase.GetCollection<DB_Player>("PlayerInfo");

            #region DBTest
            //collection.Register("hanxu", "123");

            //if (collection.IsAccountExist("hanxu"))
            //{
            //    if(collection.CheckPassword("hanxu", "123"))
            //    {
            //        Console.WriteLine("密码正确");
            //        PlayerData playerData = collection.GetPlayerData("hanxu");
            //        Console.WriteLine(playerData.text);
            //        playerData.text = "Cao";
            //        collection.UpdatePlayerData("hanxu", playerData);
            //        Console.WriteLine(collection.GetPlayerData("hanxu").text);
            //    }
            //    else
            //    {
            //        Console.WriteLine("密码错误");
            //    }
            //}
            #endregion END_DBTest

            NetManager.StartLoop(8888);
        }
    }
}
