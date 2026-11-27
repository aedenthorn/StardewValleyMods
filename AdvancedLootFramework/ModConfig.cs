namespace AdvancedLootFramework
{
	public class ModConfig
	{
		public string[] ForbiddenWeapons { get; set; } = { "32", "33", "34" };
		public string[] ForbiddenBigCraftables { get; set; } = { "22", "23", "101" };
		public string[] ForbiddenObjects { get; set; } = System.Array.Empty<string>();
	}
}
