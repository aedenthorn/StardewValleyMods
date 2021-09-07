using StardewModdingAPI;

namespace DialogueTrees
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public SButton ModButton { get; set; } = SButton.LeftAlt;
        public SButton AskButton { get; set; } = SButton.MouseLeft;
        public SButton AnswerButton { get; set; } = SButton.MouseRight;
        public int MaxPlayerQuestions { get; set; } = 4;
    }
}
