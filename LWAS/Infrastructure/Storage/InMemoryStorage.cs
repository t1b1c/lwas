using System;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web.Caching;

namespace LWAS.Infrastructure.Storage
{
    public class InMemoryStorage
    {
        public Dictionary<string, Dictionary<string, string>> Storage;

        public static InMemoryStorage Instance;

        static InMemoryStorage()
        {
            Instance = new InMemoryStorage();
            Instance.Storage = new Dictionary<string, Dictionary<string, string>>();
        }

        public Dictionary<string, string> UserData(string user)
        {
            if (!Instance.Storage.ContainsKey(user))
                Instance.Storage.Add(user, new Dictionary<string, string>());
            return Instance.Storage[user];
        }
    }
}
