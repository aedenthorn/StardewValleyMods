using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace MailboxMenu
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string InboxText { get; set; } = "Mailbox";
        public string ArchiveText { get; set; } = "Old Mail";
    }
}
