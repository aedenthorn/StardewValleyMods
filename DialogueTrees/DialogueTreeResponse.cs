using StardewValley;
using System.Collections.Generic;

namespace DialogueTrees
{
    public class DialogueTreeResponse
    {
        public DialogueTree lastTopic;
        public DialogueTree nextTopic;
        public Dictionary<string, string> topicResponses = new Dictionary<string, string>();
        public NPC npc;
        public string responseID;

        public DialogueTreeResponse(DialogueTree _lastTopic, DialogueTree _nextTopic, NPC n, string responseID)
        {
            lastTopic = _lastTopic;
            nextTopic = _nextTopic;
            topicResponses[lastTopic.topicID] = responseID;
            npc = n;
        }
    }
}