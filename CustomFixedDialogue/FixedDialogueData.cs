using System.Collections.Generic;

namespace CustomFixedDialogue
{
    internal class FixedDialogueData
    {
        public string path;
        public string text;
        public string modPath;
        public List<string> subs = new List<string>();
        public int gender = -1;

        public FixedDialogueData(string path, string text, object[] subs, int gender)
        {
            this.path = path;
            this.text = text;
            if(subs != null)
            {
                foreach(var sub in subs)
                {
                    this.subs.Add(sub + "");
                }
            }
            this.gender = gender;
        }
    }
}