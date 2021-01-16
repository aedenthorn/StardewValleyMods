namespace TrashCanReactions
{
    public class Reactor
    {
        public string dialogue;
        public int emote;
        public int points;

        public Reactor(int emote, int points, string dialogue)
        {
            this.emote = emote;
            this.points = points;
            this.dialogue = dialogue;
        }
    }
}