using System.Collections.Generic;

namespace NPCConversations
{
    public class NPCConversationData
    {
        public static string characterType;
        public static List<List<string>> participants;
        public static List<DialogueData> dialogue;
    }

    public class DialogueData
    {
        public int participant;
        public int waitTimeMin;
        public int waitTimeMax = -1;
        public int sayTime = 3000;
    }
}