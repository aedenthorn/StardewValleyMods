namespace MayoMart
{
	public partial class ModEntry
	{
		public class Dialogue_parseDialogueString_Patch
		{
			public static void Prefix(ref string masterString)
			{
				if (!Config.ModEnabled || !Config.ReplaceTexts)
					return;

				ReplaceJojaWithMayo(ref masterString);
			}
		}
	}
}
