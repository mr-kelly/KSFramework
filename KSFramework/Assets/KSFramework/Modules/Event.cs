using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSFramework
{
    /// <summary>
    /// EventEmitter for KSFramework's C# + Lua Script
    /// </summary>
    public class Event
    {
        public delegate void EventCallback(object[] args);

        /// <summary>
        /// Callback store
        /// </summary>
        private Dictionary<string, HashSet<EventCallback>> _callbacks = new Dictionary<string, HashSet<EventCallback>>();

        /// <summary>
        /// Add event with callback
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool Add(string eventName, EventCallback callback)
        {
            HashSet<EventCallback> list;
            if (!_callbacks.TryGetValue(eventName, out list))
            {
                _callbacks[eventName] = list = new HashSet<EventCallback>();
            }
            return list.Add(callback);
        }


        /// <summary>
        /// Remove a event's all callbacks, careful
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public void RemoveAll(string eventName)
        {
            HashSet<EventCallback> list;
            if (!_callbacks.TryGetValue(eventName, out list))
                _callbacks[eventName] = list = new HashSet<EventCallback>();
            list.Clear();
        }

        /// <summary>
        /// Remove a callback from event
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool Remove(string eventName, EventCallback callback)
        {
            HashSet<EventCallback> list;
            if (!_callbacks.TryGetValue(eventName, out list))
                _callbacks[eventName] = list = new HashSet<EventCallback>();
            return list.Remove(callback);
        }

    }
}
