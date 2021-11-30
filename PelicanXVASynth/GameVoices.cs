using System.Collections.Generic;

namespace PelicanXVASynth
{
    public class GameVoices
    {
        public Dictionary<string, List<Voice>> games = new Dictionary<string, List<Voice>>();
    }

    public class GameVoice
    {
        public GameVoice(string game, string id)
        {
            this.game = game;
            this.id = id;
        }
        public string game;
        public string id;
    }
    public class Voice
    {
        public string id;
        public string name;
    }
}