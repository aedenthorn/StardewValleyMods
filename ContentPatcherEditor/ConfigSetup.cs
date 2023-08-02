using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Collections.Generic;

namespace ContentPatcherEditor
{
    public class ConfigSetup
    {
        public string oldKey;
        public Dictionary<Vector2, string> labels = new();
        public TextBox Key;
        public TextBox Default;
        public List<TextBox> AllowValues = new();
        public ClickableTextureComponent DeleteCC;
        public ClickableTextureComponent AllowValuesAddCC;
        public List<ClickableTextureComponent> AllowValuesSubCCs = new();
        public ClickableTextureComponent AllowBlank;
        public ClickableTextureComponent AllowMultiple;
    }
}