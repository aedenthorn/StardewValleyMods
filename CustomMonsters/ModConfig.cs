using System.Collections.Generic;

namespace CustomMonsters
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool NPCReactAsGarbage { get; set; } = true;
		public int GhostChance { get; set; } = 25;
		public int DigChance { get; set; } = 60;
		public int ArtifactChance { get; set; } = 30;
		public List<string> NotBones { get; set; } = new List<string>()
		{
            "119"
        };
    }
}
