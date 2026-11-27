using System;
using StardewValley;
using StardewValley.TokenizableStrings;

namespace Dreamscape
{
	public class TokensUtility
	{
		public static void Register()
		{
			TokenParser.RegisterParser($"{ModEntry.context.ModManifest.UniqueID}_I18n", I18n);
		}

		private static bool I18n(string[] query, out string replacement, Random random, Farmer player)
		{
			if (!ArgUtility.TryGet(query, 1, out string key, out string error))
			{
				return TokenParser.LogTokenError(query, error, out replacement);
			}
			replacement = ModEntry.SHelper.Translation.Get(key);
			return true;
		}
	}
}
