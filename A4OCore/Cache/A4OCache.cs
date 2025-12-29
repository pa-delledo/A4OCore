
using A4OCore.Models;

namespace A4OCore.Cache
{
    internal class A4OCache
    {
        public bool DesignCacheSet(string key, Func<DesignElement> f)
        {
            if (DesignCache.ContainsKey(key)) return false;
            DesignCache.Add(key, f);

            return true;
        }
        public bool DesignCacheContainsKey(string key)
        {
            return DesignCache.ContainsKey(key);

        }
        public DesignElement DesignCacheGet(string key)
        {
            if (!DesignCache.ContainsKey(key)) return null;
            return DesignCache[key]();
        }

        private Dictionary<string, Func<DesignElement>> DesignCache = new Dictionary<string, Func<DesignElement>>();
    }
}
