using StardewModdingAPI;

namespace Swim
{
    public class SwimModApi 
    {
        public IMonitor Monitor;
        public ModEntry context; 
        public SwimModApi(IMonitor monitor, ModEntry _context)
        {
            context = _context;
            Monitor = monitor;
        }
        public bool AddContentPack(IContentPack contentPack)
        {
            try
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                DiveMapData data = contentPack.ReadJsonFile<DiveMapData>("content.json");
                SwimUtils.ReadDiveMapData(data);
                return true;
            }
            catch
            {
                Monitor.Log($"couldn't read content.json in content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                return false;
            }
        }
    }
}