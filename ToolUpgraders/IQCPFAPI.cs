using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ToolUpgraders
{
    public interface IQCPFAPI
    {
        public void StartPack();
        public void WritePack(string target);
        public void AddEditData(string target, Dictionary<string, object> entries);
        public void AddEditImage(string fromFile, string target, Rectangle? fromArea = null, Rectangle? toArea = null);
        public void AddLoad(string fromFile, string target);

    }
}