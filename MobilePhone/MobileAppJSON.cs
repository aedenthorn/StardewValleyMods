using System.Collections.Generic;

namespace MobilePhone
{
    public class MobileAppJSON
    {
        public List<AppJSON> apps;

        public string id;
        public string name;
        public string iconPath;
        public string dllName;
        public string className;
        public string methodName;
        public string keyPress;
        public bool closePhone;
    }

    public class AppJSON
    {
        public string id;
        public string name;
        public string iconPath;
        public string dllName;
        public string className;
        public string methodName;
        public string keyPress;
        public bool closePhone;
    }
}