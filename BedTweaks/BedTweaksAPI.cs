namespace BedTweaks
{
    public class BedTweaksAPI
    {
        public int GetBedWidth()
        {
            return ModEntry.Config.BedWidth;
        }
    }
}