using StardewModdingAPI;
using System.Collections.Generic;

namespace GMCMHelper
{
    public interface IGMCMHelperAPI
    {
        public bool TryAddMod(IManifest ModManifest, object Config, ITranslationHelper Translation, IModHelper Helper, IList<string> ExcludeList);
    }
    public class GMCMHelperAPI : IGMCMHelperAPI
    {
        public bool TryAddMod(IManifest ModManifest, object Config, ITranslationHelper Translation, IModHelper Helper, IList<string> ExcludeList)
        {
            return ModEntry.AddMod(ModManifest, Config, Translation, Helper, ExcludeList);
        }
    }
}