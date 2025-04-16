namespace AdvancedFluteBlocks
{
	public class AdvancedFluteBlocksApi: IAdvancedFluteBlocksApi
	{
		public string GetFluteBlockToneFromIndex(int index)
		{
			string[] tones = ModEntry.Config.ToneList.Split(',');

			if (index < tones.Length)
			{
				return tones[index];
			}
			return null;
		}
	}
}
