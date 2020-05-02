using System.Collections.Generic;

namespace RandomNPC
{
    public class DialogueData
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
        public Dictionary<string, List<string>> quiz_types;
        public string quiz_dont_know;
        public List<string> quiz_where_answers;
        public List<string> quiz_when_answers;
        public List<string> quiz_howmuch_places;
        public List<string> quiz_howmuch_items;
        public List<string> quiz_who_answers;
        public List<string> quiz_quest;
        public List<string> quizRight;
        public List<string> quizWrong;
        public List<string> quizUnknown;
    }
}