using System;
using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Events;
using StardewValley.Network.NetEvents;

namespace NightEventChanceTweak
{
	public partial class ModEntry
	{
		public class Utility_pickFarmEvent_Patch
		{
			public static void Prefix(ref bool __state)
			{
				if (!Config.ModEnabled)
					return;

				__state = Game1.getFarm().hasMatureFairyRoseTonight;
			}

			public static void Postfix(bool __state, ref FarmEvent __result)
			{
				if (!Config.ModEnabled)
					return;

				if (__result is not null and not FairyEvent and not WitchEvent)
				{
					if (__result is not SoundInTheNightEvent soundInTheNightEvent)
						return;

					int behavior = AccessTools.FieldRefAccess<SoundInTheNightEvent, NetInt>(soundInTheNightEvent, "behavior").Value;

					if (behavior != SoundInTheNightEvent.cropCircle && behavior != SoundInTheNightEvent.meteorite && behavior != SoundInTheNightEvent.owl)
						return;
				}

				Random random = Utility.CreateDaySaveRandom();

				for (int i = 0; i < 10; i++)
				{
					random.NextDouble();
				}

				double chance = random.NextDouble();
				int randomEvent = Config.CumulativeChance ? random.Next(5) : -1;
				float cumulativeChance = Config.CumulativeChance ? Config.CropFairyChance + (__state ? 0.007f : 0.0f) + Config.WitchChance + Config.MeteorChance + Config.StoneOwlChance + Config.StrangeCapsuleChance : -1f;

				SMonitor.Log("Checking for night event");
				if ((Config.CumulativeChance && randomEvent == 0) || !Config.CumulativeChance)
				{
					if (Config.IgnoreEventConditions || (!Game1.IsWinter && Game1.dayOfMonth != 1))
					{
						float cropFairyChance = Config.CumulativeChance ? cumulativeChance : Config.CropFairyChance + (__state ? 0.007f : 0.0f);

						if (chance < cropFairyChance / 100f)
						{
							__result = new FairyEvent();
							SMonitor.Log("Setting crop fairy event");
							return;
						}
					}
				}
				if ((Config.CumulativeChance && randomEvent == 1) || !Config.CumulativeChance)
				{
					if (Config.IgnoreEventConditions || Game1.stats.DaysPlayed > 20)
					{
						float witchChance = Config.CumulativeChance ? cumulativeChance : Config.WitchChance;

						if (chance < witchChance / 100f)
						{
							__result = new WitchEvent();
							SMonitor.Log("Setting witch event");
							return;
						}
					}
				}
				if ((Config.CumulativeChance && randomEvent == 2) || !Config.CumulativeChance)
				{
					if (Config.IgnoreEventConditions || Game1.stats.DaysPlayed > 5)
					{
						float meteorChance = Config.CumulativeChance ? cumulativeChance : Config.MeteorChance;

						if (chance < meteorChance / 100f)
						{
							__result = new SoundInTheNightEvent(SoundInTheNightEvent.meteorite);
							SMonitor.Log("Setting meteor event");
							return;
						}
					}
				}
				if ((Config.CumulativeChance && randomEvent == 3) || !Config.CumulativeChance)
				{
					float stoneOwlChance = Config.CumulativeChance ? cumulativeChance : Config.StoneOwlChance;

					if (chance < stoneOwlChance / 100f)
					{
						__result = new SoundInTheNightEvent(SoundInTheNightEvent.owl);
						SMonitor.Log("Setting stone owl event");
						return;
					}
				}
				if ((Config.CumulativeChance && randomEvent == 4) || !Config.CumulativeChance)
				{
					if (Config.IgnoreEventConditions || (Game1.year > 1 && !Game1.MasterPlayer.mailReceived.Contains("Got_Capsule")))
					{
						float strangeCapsuleChance = Config.CumulativeChance ? cumulativeChance : Config.StrangeCapsuleChance;

						if (chance < strangeCapsuleChance / 100f)
						{
							if (!Game1.MasterPlayer.mailReceived.Contains("Got_Capsule"))
							{
								Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "Got_Capsule", MailType.Received, add: true);
							}
							__result = new SoundInTheNightEvent(SoundInTheNightEvent.cropCircle);
							SMonitor.Log("Setting strange capsule event");
							return;
						}
					}
				}
				__result = null;
			}
		}
	}
}
