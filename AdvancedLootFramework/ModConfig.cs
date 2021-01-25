namespace AdvancedLootFramework
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int[] ForbiddenWeapons { get; set; } = { 32, 34 };
        public int[] ForbiddenBigCraftables { get; set; } = { 22, 23, 101 };
        public int[] ForbiddenObjects { get; set; } = {  };
    }
}
