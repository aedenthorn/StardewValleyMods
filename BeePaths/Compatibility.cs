namespace BeePaths
{
	internal class CompatibilityUtility
	{
		public const string wallPlantersOffsetKey = "aedenthorn.WallPlanters/offset";
		public const string wallPlantersInnerOffsetKey = "aedenthorn.WallPlanters/innerOffset";
		internal static readonly bool IsWallPlantersLoaded = ModEntry.SHelper.ModRegistry.IsLoaded("aedenthorn.WallPlanters");
	}
}
