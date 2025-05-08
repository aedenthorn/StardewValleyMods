using System.Collections.Generic;
using StardewModdingAPI;

namespace WitcherMod
{
	public partial class ModEntry
	{
		public static void ReplaceDialogues(IAssetData data, Dictionary<string, string> dialogues)
		{
			IDictionary<string, string> dict = data.AsDictionary<string, string>().Data;

			foreach (string key in dialogues.Keys)
			{
				if (dict.ContainsKey(key))
				{
					dict[key] = dialogues[key];
				}
			}
		}

		public static void ReplaceNPCNames(IAssetData data, string key, string value)
		{
			IDictionary<string, string> dict = data.AsDictionary<string, string>().Data;

			if (dict.ContainsKey(key))
			{
				dict[key] = value;
			}
		}
	}
}
