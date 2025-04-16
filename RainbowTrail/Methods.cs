using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;

namespace RainbowTrail
{
	public partial class ModEntry
	{
		private void ReloadRainbowTrailTexture(Farmer who)
		{
			rainbowTexture = Game1.content.Load<Texture2D>(rainbowTrailKey);
		}

		private static void EnableRainbowTrail(Farmer who)
		{
			if (!string.IsNullOrEmpty(Config.EnableSound))
			{
				Game1.currentLocation.playSound(Config.EnableSound);
			}
			if (!IsRainbowTrailActive(who))
			{
				Buff buff = new(buffId, buffId, effects: new BuffEffects(new BuffAttributesData() {
					Speed = Config.MoveSpeed
				}))
				{
					displayName = SHelper.Translation.Get("buff.rainbow-trail-buff.name"),
					description = SHelper.Translation.Get("buff.rainbow-trail-buff.description"),
					displaySource = SHelper.Translation.Get("buff.rainbow-trail-buff.source"),
					iconTexture = Game1.buffsIcons,
					iconSheetIndex = 9,
					millisecondsDuration = Buff.ENDLESS,
					totalMillisecondsDuration = Buff.ENDLESS,
				};

				who.buffs.Apply(buff);
			}
		}

		private static void DisableRainbowTrail(Farmer who)
		{
			if (IsRainbowTrailActive(who))
			{
				who.buffs.Remove(buffId);
			}
		}

		private static void ToggleRainbowTrail(Farmer who)
		{
			if (!IsRainbowTrailActive(Game1.player))
			{
				EnableRainbowTrail(Game1.player);
			}
			else
			{
				DisableRainbowTrail(Game1.player);
			}
		}

		private static bool IsRainbowTrailActive(Farmer who)
		{
			return who.buffs.AppliedBuffIds.Contains(buffId);
		}

		private void ClearRainbowTrail(Farmer who)
		{
			trailDictionary.Remove(who.UniqueMultiplayerID);
		}
	}
}
