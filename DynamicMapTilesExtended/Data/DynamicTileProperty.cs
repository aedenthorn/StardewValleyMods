namespace DMT.Data
{
    public class DynamicTileProperty
    {
        public string logName = "";
        public string LogName
        {
            get => logName; 
            set => logName = value;
        }

        public string key;
        public string Key
        {
            get => key;
            set => key = value;
        }

        public string value;
        public string Value
        {
            get => value;
            set => this.value = value;
        }

        public string trigger;
        public string Trigger
        {
            get => trigger;
            set => trigger = value;
        }

        public bool once = false;
        public bool Once
        {
            get => once;
            set => once = value;
        }
        public bool swap = false;
        public bool Swap
        {
            get => swap;
            set => swap = value;
        }
        public string invalidate = "None";
        public string Invalidate
        {
            get => invalidate;
            set => invalidate = value;
        }
    }
}
