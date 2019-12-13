using System;

namespace RandomNPC
{
    public class RNPC
    {
        internal string nameID;
        public string npc;
        public string birthday;
        public string startLoc;
        public string name;
        public string skin;
        public string age;
        public string manner;
        public string anxiety;
        public string optimism;
        public string gender;
        public string datable;
        public string refinement;
        internal string[] giftTaste;

        public RNPC(string npc)
        {
            this.npc = npc;
            string[] npca = npc.Split('/');
            this.age = npca[0];
            this.manner = npca[1];
            this.anxiety = npca[2];
            this.optimism = npca[3];
            this.gender = npca[4];
            this.datable = npca[5];
            this.birthday = npca[8];
            this.startLoc = npca[10];
            this.name = npca[11];
            this.skin = npca[12];
            this.refinement = npca[13];
            this.giftTaste = npca[14].Split('^');
            
            this.nameID = String.Join("", name.Split(' '));
        }
    }
}