using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebUI.Interfaces
{
    public interface ICacheProvider
    {
        T Get<T>(string key);
        void Add(string key, object obj);
        void Remove(string key);
    }
}
