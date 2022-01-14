using System.Collections.Generic;

namespace PlayerAnimationFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static bool ChatBox_runCommand_Prefix(string command)
        {
            if (!Config.EnableMod)
                return true;

            if(command.StartsWith("paf "))
            {
                var parts = command.Split(' ');
                int l = 1000;
                if(int.TryParse(parts[1], out int f) && (parts.Length == 2 || int.TryParse(parts[2], out l)))
                {
                    PlayerAnimation pad = new PlayerAnimation()
                    {
                        animations = new List<PlayerAnimationFrame>()
                        {
                            new PlayerAnimationFrame()
                            {
                                frame = f,
                                length = l
                            }
                        }
                    };
                    PlayAnimation(command, pad);
                    return false;
                }

            }

            LoadAnimations();
            foreach (var kvp in animationDict)
            {
                if (kvp.Value.chatTrigger != null && kvp.Value.chatTrigger == command)
                {
                    PlayAnimation(kvp.Key, kvp.Value);
                    return false;
                }
            }
            return true;
        }
    }
}