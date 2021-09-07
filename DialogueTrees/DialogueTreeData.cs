using System.Collections.Generic;

namespace DialogueTrees
{
    public class DialogueTreeData
    {
        public List<DialogueTree> dialogues = new List<DialogueTree>();
        public Dictionary<string, string> topicNames = new Dictionary<string, string>();
        public Dictionary<string, string> standardQuestions = new Dictionary<string, string>();
        public Dictionary<string, string> playerResponses = new Dictionary<string, string>();
    }
    public class DialogueTree
    {
        public string topicID;
        public bool isStarter = true;
        public bool playerCanAsk = true;
        public float followupChance = 1;
        public Dictionary<string, List<string>> requiredResponses = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> nextTopics = new Dictionary<string, List<string>>();
        public List<string> questionIDs;
        public List<string> responseIDs;
    }
}