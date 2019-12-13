using StardewValley;
using System;

namespace RandomNPC
{
    public class RNPCSchedule
    {
        public string morningLoc;
        public string afternoonLoc;
        internal string morningEarliest;
        internal string afternoonEarliest;
        private RNPC npc;

        public RNPCSchedule(RNPC npc)
        {
            this.npc = npc;
        }

        public string ID { get; internal set; }

        public string MakeString()
        {
            string str = "";
            string mTime;
            int mHour;
            int mH;
            int mM;
            if(morningEarliest != "any")
            {
                mHour = int.Parse(morningEarliest.Substring(0,morningEarliest.Length == 4?2:1));
                mM = (int.Parse(morningEarliest) % 100) / 10;
                mH = Game1.random.Next(mHour, Math.Min(mHour,9));
                mM = Game1.random.Next(mH == mHour?mM:0, 5);
            }
            else
            {
                mH = Game1.random.Next(6, 9);
                mM = Game1.random.Next(0, 5);
            }
            mTime = mH.ToString() + mM.ToString() + "0";

            string aTime;
            int aH;
            int aHour;
            int aM;
            if(afternoonEarliest != "any" && int.Parse(afternoonEarliest) > 1200)
            {
                aHour = (int)Math.Round((double)(double.Parse(afternoonEarliest)/100));
                aM = (int.Parse(afternoonEarliest) % 100) / 10;
                aH = Game1.random.Next(aHour, Math.Min(aHour, 16));
                aM = Game1.random.Next((aH == aHour?aM:0), 5);
            }
            else
            {
                aH = Game1.random.Next(12, 16);
                aM = Game1.random.Next(0, 5);
            }
            aTime = aH.ToString() + aM.ToString() + "0";

            str += mTime + " " + morningLoc + "/" + aTime + " " + afternoonLoc;
            return str;
        }
    }
}