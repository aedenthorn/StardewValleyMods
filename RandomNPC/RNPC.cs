using System;

namespace RandomNPC
{
    public class RNPC
    {
        public string nameID;
        public string npcString;
        public string birthday;
        public string startLoc;
        public string name;
        public string skinColour;
        public string hairStyle;
        public string hairColour;
        public string eyeColour;
        public string age;
        public string manner;
        public string anxiety;
        public string optimism;
        public string gender;
        public string datable;
        public string[] traits;
        public string[] giftTaste;
        public string bodyType;
        public bool visiting = false;
        public string[] clothes = null;
        public string[] topRandomColour = null;
        public RNPCSchedule schedule;

        public RNPC(string npcString, string npcID)
        {
            this.npcString = npcString;
            string[] npca = npcString.Split('/');
            int i = 0;
            this.age = npca[i++];
            this.manner = npca[i++];
            this.anxiety = npca[i++];
            this.optimism = npca[i++];
            this.gender = npca[i++];
            this.datable = npca[i++];
            this.traits = npca[i++].Split('|');
            this.birthday = npca[i++];
            this.name = npca[i++];
            this.giftTaste = npca[i++].Split('^');
            this.bodyType = npca[i++];
            this.skinColour = npca[i++];
            this.hairStyle = npca[i++];
            this.hairColour = npca[i++];
            this.eyeColour = npca[i++];

            this.nameID = npcID;
        }

        internal string MakeDisposition()
        {
            return age + "/" + manner + "/" + anxiety + "/" + optimism + "/" + gender + "/" + datable + "//Other/" + birthday + "//" +startLoc+ "/" + name;
        }
    }
}