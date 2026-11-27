namespace InstantBuildingConstructionAndUpgrade
{
	internal class CompatibilityUtility
	{
		internal static readonly bool IsSolidFoundationsLoaded = ModEntry.SHelper.ModRegistry.IsLoaded("PeacefulEnd.SolidFoundations");
	}
}
