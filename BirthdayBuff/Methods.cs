using StardewValley;
using System.Text;

namespace BirthdayBuff
{
    public partial class ModEntry
    {
        private string GetBuffDescription()
        {
            StringBuilder b = new StringBuilder();
            if (Config.Farming != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.480"), Config.Farming));
            }
            if (Config.Fishing != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.483"), Config.Fishing));
            }
            if (Config.Mining != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.486"), Config.Mining));
            }
            if (Config.Luck != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.489"), Config.Luck));
            }
            if (Config.Foraging != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.492"), Config.Foraging));
            }
            if (Config.MaxStamina != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.495"), Config.MaxStamina));
            }
            if (Config.MagneticRadius != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.498"), Config.MagneticRadius));
            }
            if (Config.Defense != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.501"), Config.Defense));
            }
            if (Config.Attack != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.504"), Config.Attack));
            }
            if (Config.Speed != 0)
            {
                b.AppendLine(GetBuffLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.507"), Config.Speed));
            }
            return b.ToString();
        }

        private string GetBuffLine(string v, int c)
        {
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
            {
                return v + (c > 0 ? "+" : "-") + c;
            }
            else
            {
                return (c > 0 ? "+" : "-") + c + v;
            }
        }
    }
}