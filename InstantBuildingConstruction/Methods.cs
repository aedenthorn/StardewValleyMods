using StardewModdingAPI;
using System;
using System.Reflection;

namespace InstantBuildingConstructionAndUpgrade
{
	public partial class ModEntry
	{
		public static void ExecuteCommand(string command)
		{
			Type sCoreType = Type.GetType("StardewModdingAPI.Framework.SCore, StardewModdingAPI");
			Type commandQueueType = Type.GetType("StardewModdingAPI.Framework.CommandQueue, StardewModdingAPI");
			object instance = sCoreType.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			object rawCommandQueue = sCoreType.GetField("RawCommandQueue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
			MethodInfo addMethod = commandQueueType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

			addMethod.Invoke(rawCommandQueue, new object[] { command });
		}

        private static void InvalidateCaches()
        {
            SHelper.GameContent.InvalidateCache(asset => asset.Name.IsEquivalentTo("Data/Buildings"));
            SHelper.GameContent.InvalidateCache(asset => asset.Name.IsEquivalentTo("Data/HomeRenovations"));
            SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Strings/Locations"));
            SMonitor.Log("Modifications to building data and farmhouse renovation data reapplied.", LogLevel.Info);
        }

    }
}
