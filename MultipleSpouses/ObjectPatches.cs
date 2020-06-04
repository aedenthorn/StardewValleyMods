using Harmony;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Reflection;

namespace MultipleSpouses
{
	public partial class ModEntry
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		
		[HarmonyPatch(typeof(NPC), "getSpouse")]
		static class NPC_getSpouse
		{
			static bool Prefix(NPC __instance, ref Farmer __result)
			{
				foreach (Farmer f in Game1.getAllFarmers())
                {
					if(f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
                    {
						__result = f;
						return false;
					}
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(NPC), "tryToReceiveActiveObject")]
		static class tryToReceiveActiveObject
		{
			static bool Prefix(NPC __instance, ref Farmer who, string __state)
			{
				try
				{
					if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
					{
						who.spouse = __instance.Name;
					}

					if (who.ActiveObject.ParentSheetIndex == 460)
					{
						if (!__instance.datable || __instance.isMarriedOrEngaged() || (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < 1500))
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
								return false;
							}
							__instance.CurrentDialogue.Push(new Dialogue((__instance.Gender == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3970") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3971"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
						else if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < 2500)
						{
							if (!who.friendshipData[__instance.Name].ProposalRejected)
							{
								__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), __instance));
								Game1.drawDialogue(__instance);
								who.changeFriendship(-20, __instance);
								who.friendshipData[__instance.Name].ProposalRejected = true;
								return false;
							}
							__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), __instance));
							Game1.drawDialogue(__instance);
							who.changeFriendship(-50, __instance);
							return false;
						}
						else
						{
							if (!__instance.datable || who.houseUpgradeLevel >= 1)
							{
								typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, false });
								return false;
							}
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
								return false;
							}
							__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
					}
					else
					{
						return true;
					}
				}
				catch (Exception ex)
				{
					Monitor.Log($"Failed in {nameof(tryToReceiveActiveObject)}:\n{ex}", LogLevel.Error);
					return true;
				}
			}
		}
	}
}