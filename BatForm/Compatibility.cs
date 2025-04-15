namespace BatForm
{
	internal class CompatibilityUtility
	{
		internal static readonly bool IsManaBarLoaded = ModEntry.SHelper.ModRegistry.IsLoaded("spacechase0.ManaBar");
		internal static readonly bool IsZoomLevelLoaded = ModEntry.SHelper.ModRegistry.IsLoaded("thespbgamer.ZoomLevel");
	}
}
