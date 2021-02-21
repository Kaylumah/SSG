using System.Collections.Generic;

namespace Kaylumah.Ssg.Manager.Site.Service
{
    public static class DictionaryExtensions
    {
        public static T GetValue<T>(this Dictionary<string, object> dictionary, string key) where T : class
        {
            dictionary.TryGetValue(key.ToLower(), out object o);
            if (o is T t)
            {
                return t;
            }
            return null;
        }

        public static void SetValue(this Dictionary<string, object> dictionary, string key, object value)
        {
            dictionary[key.ToLower()] = value;
        }
    }
}