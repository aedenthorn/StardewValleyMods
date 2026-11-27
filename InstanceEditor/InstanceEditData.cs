using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InstanceEditor
{
    public class InstanceEditData
    {
        public string className = "";
        public Dictionary<string, FieldEditData> matchFields = new Dictionary<string, FieldEditData>();
        public Dictionary<string, FieldEditData> changeFields = new Dictionary<string, FieldEditData>();
        public string[] checks = new string[] { };
    }

    public class FieldEditData
    {
        public object value;
        public FieldInfo fieldInfo;
        public Dictionary<string, object> fields;
    }
}