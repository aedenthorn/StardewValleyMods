using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace QuickContentPatcherFile
{
    public interface IQCPFAPI
    {
        public void StartPack();
        public void WritePack(string target);
        public void AddEditData(string target, Dictionary<string, object> entries);
        public void AddEditImage(string fromFile, string target, Rectangle? fromArea = null, Rectangle? toArea = null);
        public void AddLoad(string fromFile, string target);

    }
    public class QCPFAPI : IQCPFAPI
    {
        public ContentPatcherPack pack = new();
        public void StartPack()
        {
            pack = new();
        }
        public void WritePack(string target)
        {
            File.WriteAllText(target, JsonConvert.SerializeObject(pack, Formatting.Indented));
        }
        public void AddEditData(string target, Dictionary<string, object> entries)
        {
            pack.Changes.Add(new EditDataChange() { Target = target, Entries = entries });
        }
        public void AddEditImage(string fromFile, string target, Rectangle? fromArea = null, Rectangle? toArea = null)
        {
            pack.Changes.Add(new EditImageChange() { FromFile = fromFile, Target = target, FromArea = fromArea, ToArea = toArea });
        }
        public void AddLoad(string fromFile, string target)
        {
            pack.Changes.Add(new EditImageChange() { FromFile = fromFile, Target = target});
        }
    }
}