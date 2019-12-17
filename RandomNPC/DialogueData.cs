using System.Collections.Generic;

namespace RandomNPC
{
    internal class DialogueData
    {
        public List<string> introductions = new List<string>();
        public List<string> questions = new List<string>();
        public List<string> farmer_questions = new List<string>();
        public List<string> info_responses = new List<string>();
        public List<string> rejections = new List<string>();
        public List<string> manner = new List<string>();
        public List<string> anxiety = new List<string>();
        public List<string> optimism = new List<string>();
        public List<string> advice = new List<string>();
        public List<string> datable = new List<string>();
        public Dictionary<string, List<string>> places;
        public List<string> schedules;
        public List<string> quests;
        public List<string> questRight;
        public List<string> questWrong;
        public List<string> questUnknown;
    }
}