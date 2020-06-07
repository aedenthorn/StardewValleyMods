using StardewModdingAPI;
using StardewValley;
using System;

namespace FriendlyDivorce
{
    public static class ObjectPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}
		public static void Farmer_doDivorce_Prefix(ref Farmer __instance, ref Friendship __state)
		{
            try
            {
				if (__instance.spouse != null)
				{
					__state = __instance.friendshipData[__instance.getSpouse().name];
				}
				else if (__instance.team.GetSpouse(__instance.UniqueMultiplayerID) != null)
				{
					long spouseID = __instance.team.GetSpouse(__instance.UniqueMultiplayerID).Value;
					__state = __instance.team.GetFriendship(__instance.UniqueMultiplayerID, spouseID);
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Prefix)}:\n{ex}", LogLevel.Error);
			}
		}

		public static void Farmer_doDivorce_Postfix(ref Farmer __instance, ref Friendship __state)
		{
            try
            {
				__state.Points = ModEntry.Config.PointsAfterDivorce;
				__state.Status = FriendshipStatus.Friendly;
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Postfix)}:\n{ex}", LogLevel.Error);
			}

		}

	}
}
