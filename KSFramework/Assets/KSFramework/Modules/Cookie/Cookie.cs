using System;
using UnityEngine;
using System.Collections;

namespace KSFramework
{
    /// <summary>
    /// Cookie for game developement,  such as a cache
    /// </summary>
    public class Cookie
    {
        protected static Hashtable _hashtable = new Hashtable();

        /// <summary>
        /// Set value for cookie
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, object value)
        {
            _hashtable[key] = value;
        }

        /// <summary>
        /// Get value from cookie, if null, do the action get default value!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ifNullSetDefault"></param>
        /// <returns></returns>
        public static object Get(string key, Func<object> ifNullSetDefault)
        {
            if (!_hashtable.ContainsKey(key))
            {
                _hashtable[key] = ifNullSetDefault();
            }

            return _hashtable[key];
        }
        /// <summary>
        /// Get value from cookie
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(string key)
        {
            if (_hashtable.ContainsKey(key))
                return _hashtable[key];
            return null;
        }

    }
}
