using StardewValley;
using System.Collections.Generic;

namespace ConversationBubbles
{
    public class ConversationInstance
    {
        public ConversationData data;
        public int index;
        public List<object> participants = new List<object>();
        public List<DialogueInstance> dialogues = new List<DialogueInstance>();
        public object Participant
        {
            get
            {
                return participants[dialogues[index].data.participant];
            }
        }

        public DialogueInstance Dialogue
        {
            get
            {
                return dialogues[index];
            }
            set
            {
                dialogues[index] = value;
            }
        }

    }
    public class DialogueInstance
    {
        public float textAboveHeadAlpha = 1;
        public int textAboveHeadPreTimer;
        public int textAboveHeadTimer;
        public DialogueData data;
    }

    public class ConversationData
    {
        public List<ParticipantData> participants;
        public List<DialogueData> dialogues;
        public List<string> locations;
        public int minTime = -1;
        public int maxTime = -1;
        public float chance;
        public int tileDistance;
        
    }
    public class ParticipantData
    {
        public List<string> participantTypes;
        public List<string> participantNames;
    }

    public class DialogueData
    {
        public string text;
        public int participant;
        public int waitTimeMin;
        public int waitTimeMax = -1;
        public int sayTime = 3000;
        public int style;
        public int color = -1;

    }
}