using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyDivorce
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public int PointsAfterDivorce { get; set; } = 2000;
        public bool ComplexDivorce { get; set; } = true;
    }
}
