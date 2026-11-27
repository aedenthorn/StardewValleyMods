using System.Collections.Generic;

namespace TrashCanReactions
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public string LinusDialogue { get; set; } = "Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus";
        public string ChildDialogue { get; set; } = "Data\\ExtraDialogue:Town_DumpsterDiveComment_Child";
        public string TeenDialogue { get; set; } = "Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen";
        public string AdultDialogue { get; set; } = "Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult";
        public int LinusEmote { get; set; } = 32;
        public int ChildEmote { get; set; } = 28;
        public int TeenEmote { get; set; } = 8;
        public int AdultPoints { get; set; } = -25;
        public int LinusPoints { get; set; } = 5;
        public int ChildPoints { get; set; } = -25;
        public int TeenPoints { get; set; } = -25;
        public int AdultEmote { get; set; } = 12;
        public Dictionary<string, Reactor> SpecificCharacters { get; set; } = new Dictionary<string, Reactor>()
        {
            { "ExampeNPCName", new Reactor(32, 10, "Characters\\Dialogue\\ExampleNPC:Town_DumpsterDiveComment") }
        };
    }
}
