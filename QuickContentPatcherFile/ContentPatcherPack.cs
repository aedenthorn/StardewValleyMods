using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace QuickContentPatcherFile
{
    public class ContentPatcherPack
    {
        public string Format  => ModEntry.SHelper.ModRegistry.Get("Pathoschild.ContentPatcher").Manifest.Version.ToString();
        public List<Change> Changes { get; set; } = new();

    }

    public class Change
    {
        public string Action { get; set; }
        public string Target { get; set; }
    }

    public class LoadChange : Change
    {
        public LoadChange()
        {
            Action = "Load";
        }
        public string FromFile { get; set; }
    }

    public class EditDataChange : Change
    {
        public EditDataChange()
        {
            Action = "EditData";
        }
        public Dictionary<string, object> Entries { get; set; }
    }
    public class EditImageChange : Change
    {
        public EditImageChange()
        {
            Action = "EditImage";
        }
        public string FromFile { get; set; }
        public Rectangle? FromArea { get; set; }
        public Rectangle? ToArea { get; set; }
    }
}