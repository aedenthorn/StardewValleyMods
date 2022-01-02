using StardewModdingAPI;

namespace Tent
{
	public class MyManifestContentPackFor : IManifestContentPackFor
	{
		public string UniqueID { get; set; }

		public ISemanticVersion MinimumVersion { get; set; }
	}
}