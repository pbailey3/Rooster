using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using WebUI.Interfaces;
using StackExchange.Redis;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;

namespace WebUI.Common
{
    public class RedisCacheProvider : ICacheProvider
    {
        private static readonly ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["RedisCache"].ConnectionString);

        public T Get<T>(string key)
        {
            IDatabase cache = connection.GetDatabase();
            return Deserialize<T>(cache.StringGet(key));

        }

        public void Add(string key, object obj)
        {
            IDatabase cache = connection.GetDatabase();
            cache.StringSet(key, JsonConvert.SerializeObject(obj), TimeSpan.FromMinutes(90));
        }   

        public void Remove(string key)
        {
            IDatabase cache = connection.GetDatabase();
            //TODO
        }

        private string Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }
            else
            
            return JsonConvert.SerializeObject(o);
            //BinaryFormatter binaryFormatter = new BinaryFormatter();
            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    binaryFormatter.Serialize(memoryStream, o);
            //    byte[] objectDataAsStream = memoryStream.ToArray();
            //    return objectDataAsStream;
            //}
        }

        private T Deserialize<T>(string jsonString)
        {
            if (String.IsNullOrEmpty(jsonString))
                return default(T);

            return JsonConvert.DeserializeObject<T>(jsonString);

            //BinaryFormatter binaryFormatter = new BinaryFormatter();
            //using (MemoryStream memoryStream = new MemoryStream(stream))
            //{
            //    T result = (T)binaryFormatter.Deserialize(memoryStream);
            //    return result;
            //}
        }

    }
}