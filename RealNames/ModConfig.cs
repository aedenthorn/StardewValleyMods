using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealNames
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public string LocaleString { get; set; } = "en-US";
        public string NeutralNameGender { get; set; } = "male/female";
        public bool RealNamesForAnimals { get; set; } = true;
    }
}
