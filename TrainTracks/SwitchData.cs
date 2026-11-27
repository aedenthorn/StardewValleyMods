using System.Collections.Generic;

namespace TrainTracks
{
    public class SwitchData
    {
        public int[][] trackSwitches = new int[][] { null, null, null, null };
        public SwitchData(string switchString)
        {
            string[] switches = switchString.Split(',');
            foreach(string s in switches)
            {
                string[] sw = s.Split(':');
                if (sw.Length != 2 || !int.TryParse(sw[0], out int index) || index > 3)
                    continue;
                List<int> dirs = new List<int>();
                foreach(string ds in sw[1].Split(' '))
                {
                    if(int.TryParse(ds, out int dir) && dir < 4)
                        dirs.Add(dir);
                }
                trackSwitches[index] = dirs.ToArray();
            }
        }
    }
}