using System;
using System.Collections.Generic;

namespace BirthdayBuff
{
	public interface IBuffFrameworkAPI
	{
		public bool Add(string key, Dictionary<string, object> value, Func<bool> function = null);
		public bool Remove(string key);
		public void UpdateBuffs();
	}
}
