using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using WebUI.Interfaces;

namespace WebUI.Common
{
    public class CacheManager
    {
        public static string CACHE_KEY_BUSINESS = "Bus:";
        public static string CACHE_KEY_BUSINESS_LOCATION = "BusLoc:";
        public static string CACHE_KEY_BUSINESS_TYPES = "BusTypes";
        public static string CACHE_KEY_USER_PREFS = "UserPref:";
        
        //public static string CACHE_KEY_BUSINESS_LOCATION_PREFERENCES = "BusLocPrefs";


        #region Singleton

        protected CacheManager(ICacheProvider provider)
        {
            this.cacheProvider = provider;
        }

        public static CacheManager Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
                ICacheProvider cacheProvider = null;

                var cacheProviderConfig = ConfigurationManager.AppSettings.Get("CacheProvider");
                System.Diagnostics.Trace.TraceInformation("CacheManager()-> Nested() : CacheProvider = '" + cacheProviderConfig + "'");

                switch(cacheProviderConfig)
                {
                     case "None":
                        cacheProvider = new NoCacheProvider();
                        break;
                    case "MemoryCacheProvider":
                        cacheProvider = new MemoryCacheProvider();
                        break;
                    case "RedisCacheProvider":
                        cacheProvider = new RedisCacheProvider();
                        break;
                    default:
                       Trace.TraceError("Unsupported cacheprovider value found in web config: '" + cacheProviderConfig + "'");
                        break;
                }

                instance = new CacheManager(cacheProvider);
               
            }
            internal static readonly CacheManager instance; 
        }

        #endregion

        private ICacheProvider cacheProvider;

        public T Get<T>(string key)
        {
            var obj = cacheProvider.Get<T>(key);
            if(obj != null)
                System.Diagnostics.Trace.TraceInformation("Cache key found ('" + key+"')");
            return obj;

        }

        public void Add(string key, object obj)
        {
            cacheProvider.Add(key, obj);
        }

        public void Remove(string key)
        {
            cacheProvider.Remove(key);
        }
       
    }
}