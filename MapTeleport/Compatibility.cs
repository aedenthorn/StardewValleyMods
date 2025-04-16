namespace MapTeleport
{
	internal class CompatibilityUtility
	{
		internal static readonly bool IsSVELoaded = ModEntry.SHelper.ModRegistry.IsLoaded("FlashShifter.SVECode");
		internal static readonly bool IsESLoaded = ModEntry.SHelper.ModRegistry.IsLoaded("atravita.EastScarp");
	}
}
