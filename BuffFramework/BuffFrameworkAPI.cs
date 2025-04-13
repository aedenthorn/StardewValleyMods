using System;
using System.Collections.Generic;

namespace BuffFramework
{
	public interface IBuffFrameworkAPI
	{
		public bool Add(string key, Dictionary<string, object> value, Func<bool> function = null);
		public bool Remove(string key);
		public void UpdateBuffs();
	}

	public class BuffFrameworkAPI : IBuffFrameworkAPI
	{
		public Dictionary<string, (Dictionary<string, object>, Func<bool>)> dictionary = new();

		public bool Add(string key, Dictionary<string, object> value, Func<bool> function = null)
		{
			return dictionary.TryAdd(key, (value, function));
		}

		public bool Remove(string key)
		{
			ModEntry.buffDictionary.Remove(key);
			return dictionary.Remove(key);
		}

		public void UpdateBuffs()
		{
			ModEntry.invokeUpdateBuffsOnNextTick = true;
		}
	}
}
