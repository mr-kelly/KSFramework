// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.



namespace SLua
{
	using System;
	using System.Collections.Generic;

	public class WeakDictionary<K, V>
	{
		Dictionary<K, WeakReference> _dict = new Dictionary<K, WeakReference>();

		public V this[K key]
		{
			get
			{
				WeakReference w = _dict[key];
				if (w.IsAlive)
					return (V)w.Target;
				return default(V);
			}

			set
			{
				Add(key, value);
			}
		}


		ICollection<K> Keys
		{
			get
			{
				return _dict.Keys;
			}
		}
		ICollection<V> Values
		{
			get
			{
				List<V> l = new List<V>();
				foreach (K key in _dict.Keys)
				{
					l.Add((V)_dict[key].Target);
				}
				return l;
			}
		}

		void Add(K key, V value)
		{
			if (_dict.ContainsKey(key))
			{
				if (_dict[key].IsAlive)
					throw new ArgumentException("key exists");

				_dict[key].Target = value;
			}
			else
			{
				WeakReference w = new WeakReference(value);
				_dict.Add(key, w);
			}
		}

		bool ContainsKey(K key)
		{
			return _dict.ContainsKey(key);
		}
		bool Remove(K key)
		{
			return _dict.Remove(key);
		}
		bool TryGetValue(K key, out V value)
		{
			WeakReference w;
			if (_dict.TryGetValue(key, out w))
			{
				value = (V)w.Target;
				return true;
			}
			value = default(V);
			return false;

		}
	}
}