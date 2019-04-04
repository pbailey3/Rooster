using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using WebUI.Interfaces;

namespace WebUI.Common
{
    public class MemoryCacheProvider : ICacheProvider
    {
        private static MemoryCache cache = MemoryCache.Default;
    
        public T Get<T>(string key)
        {
            return (T)cache[key]; 
        }

        public void Add(string key, object obj)
        {
            cache.Set(key, obj, DateTimeOffset.Now.AddMinutes(90)); 
        }

        public void Remove(string key)
        {
            if (cache.Contains(key))
                cache.Remove(key);
        }
    }
}