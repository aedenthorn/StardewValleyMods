namespace AdvancedFluteBlocks
{
    public class AdvancedFluteBlocksApi
    {
        public string GetFluteBlockToneFromIndex(int index)
        {
            var tones = ModEntry.Config.ToneList.Split(',');
            if (index >= tones.Length)
                return null;
            return tones[index];
        }
    }
}