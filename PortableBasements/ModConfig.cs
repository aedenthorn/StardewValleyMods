using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace PortableBasements
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
        public string LadderCost { get; set; } = "388 50 390 50";
        public string SkillReq { get; set; } = "Mining 2";
    }
}
