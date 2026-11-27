using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewVN
{
    public class VisualNovelData
    {
        public List<VNOption> OpeningScenes { get; set; } = new List<VNOption>();
        public Dictionary<string, VNVariable> Variables { get; set; } = new Dictionary<string, VNVariable>();
        public Dictionary<string, VNScene> Scenes { get; set; } = new Dictionary<string, VNScene>();
        public Dictionary<string, VNObject> Objects { get; set; } = new Dictionary<string, VNObject>();
        public Dictionary<string, VNDialogue> Dialogues { get; set; } = new Dictionary<string, VNDialogue>();
        public Dictionary<string, VNAction> Actions { get; set; } = new Dictionary<string, VNAction>();
        public Dictionary<string, string> Texts { get; set; } = new Dictionary<string, string>();

    }
}