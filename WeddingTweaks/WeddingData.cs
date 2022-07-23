using System.Collections.Generic;

namespace WeddingTweaks
{
    public class WeddingData
    {
        public List<string> witnesses = new List<string>();
        public List<string> witnessAcceptDialogue = new List<string>();
        public List<string> witnessDeclineDialogue = new List<string>();
        public int witnessFrame = -1;
        public int witnessAcceptChance = -1;
    }
}