using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using WebUI.Interfaces;

namespace WebUI.Common
{
    public class NoCacheProvider : ICacheProvider
    {
        public T Get<T>(string key)
        {
            return default(T);
        }

        public void Add(string key, object obj)
        {
           //Do Nothing
        }

        public void Remove(string key)
        {
            //Do Nothing
        }
    }
}