using System.Collections.Generic;

namespace ProceduralDialogue
{
    public class ProceduralDialogueData
    {
        public List<ProceduralDialogue> dialogues = new List<ProceduralDialogue>();
        public Dictionary<string, string> topicNames = new Dictionary<string, string>();
        public Dictionary<string, string> playerQuestions = new Dictionary<string, string>();
        public Dictionary<string, string> playerResponses = new Dictionary<string, string>();
        public Dictionary<string, string> UIStrings = new Dictionary<string, string>();
    }
    public class ProceduralDialogue
    {
        public string topicID;
        public List<string> questionIDs;
        public List<string> responseIDs;
    }
}