using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace C7_Server
{
    public static class DBManager
    {
        public static MongoClient mongoClient;

        public static IMongoDatabase Connect(string ip, int port, string databaseName)
        {
            string connectStr = $"mongodb://{ip}:{port}";
            mongoClient = new MongoClient(connectStr);
            IMongoDatabase dataBase = mongoClient.GetDatabase(databaseName);
            return dataBase;
        }

        /// <summary>
        /// 获得表
        /// </summary>
        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase database, string collection = null)
        {
            return database.GetCollection<T>(collection ?? typeof(T).Name);
        }

        public static T Query<T>(this IMongoCollection<T> database, Expression<Func<T, bool>> filter)
        {
            return database.Find(filter).FirstOrDefault();
        }
    }
}
