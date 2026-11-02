namespace SittingAction
{
    public partial class ModEntry
    {

        public static bool SwitchBool(bool value)
        {
            if(!value || !Config.ModEnabled || SHelper.Input.IsDown(Config.SuppressKey))
                return value;
            return false;
        }
    }
}