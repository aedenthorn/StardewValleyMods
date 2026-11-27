

using HarmonyLib;
using Netcode;
using StardewValley;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using static StardewValley.Minigames.TargetGame;

namespace CLCSharp
{
    public partial class ModEntry
    {
        public enum CommandType
        {
            None,
            Get,
            Set,
            Invoke
        }
        private Type rootType = null;
        private void ParseCommand(string command, string[] args)
        {
            rootType = null;
            object toSet = null;
            object toSetParent = null;
            string toSetKey = null;
            string toSetName = null;
            var a = string.Join(" ", args);
            var name = "";
            var part = "";
            bool keying = false;
            bool quoting = false;
            bool commanding = false;
            bool setting = false;
            string currentSub = "";
            object member = null;
            object parent = null;
            for (int i = 0; i < a.Length; i++)
            {
                var c = a[i];

                if(!keying && c == '[')
                {
                    member = GetMember(parent, ref part);
                    keying = true;
                    goto cont;
                }
                else if(!quoting && c == '"')
                {
                    quoting = true;
                }
                else if(!commanding && c == '(')
                {
                    commanding = true;
                }
                else if(keying)
                {
                    if (c == ']')
                    {
                    }
                    else
                    {
                        currentSub += c;
                        goto cont;
                    }
                }
                else if(quoting)
                {
                    if(c == '"')
                    {
                    }
                    else
                    {
                        currentSub += c;
                    }
                }
                else if(commanding)
                {
                    if(c == ')')
                    {
                        currentSub = "";
                        member = AccessTools.Method(parent.GetType(), currentSub);
                        commanding = false;
                    }
                    else
                    {
                        currentSub += c;
                    }
                }
                else if (c == ' ')
                {
                    continue;
                }
                else if (c == '.')
                {
                    if (part == null)
                    {
                        SMonitor.Log("Syntax error, invalid '.'.");
                        return;
                    }
                    member = GetMember(parent, ref part);
                    if (rootType == null)
                    {
                        rootType = AccessTools.TypeByName(name);
                        goto cont;
                    }
                }
                else if (c == '=')
                {
                    if (!setting)
                    {
                        setting = true;
                        toSetParent = parent;
                        toSet = !string.IsNullOrWhiteSpace(currentSub) ? GetDictType(parent) : GetMember(parent, ref part);
                        toSetKey = currentSub;
                        toSetName = name;
                        name = "";
                        rootType = null;
                        parent = null;
                        member = null;
                        part = "";
                        var value = a[(i + 1)..].Trim();
                        if(!value.Contains('.') || value.Contains(' '))
                        {
                            SetCSValue(toSetParent, toSet, toSetKey, parent, value, toSetName);
                            return;
                        }
                        continue;
                    }
                    else
                    {
                        SMonitor.Log("Syntax error, too many '='.");
                        return;
                    }
                }
                else
                {
                    part += a[i];
                    if (i < a.Length - 1)
                    {
                        goto cont;
                    }
                    else
                    {
                        name += c;
                        member = GetMember(parent, ref part);
                    }
                }
                if (member is null)
                {
                    SMonitor.Log($"{name} not found.");
                    return;
                }
                if (i == a.Length - 1 && setting)
                {
                    SetCSValue(toSetParent, toSet, currentSub, parent, member, toSetName);
                    return;
                }
                else if(member is FieldInfo fi)
                {
                    parent = fi.GetValue(parent);
                }
                else if(member is PropertyInfo pi)
                {
                    parent = pi.GetValue(parent);
                }
                else if (member is MethodInfo mi)
                {
                    parent = mi.Invoke(parent, Array.Empty<object>());
                }
                if (parent == null)
                {
                    SMonitor.Log($"{name} is null.");
                    return;
                }
                if (keying)
                {
                    parent = GetDictValue(parent, currentSub);
                    currentSub = "";
                    keying = false;
                }

            cont:
                name += c;
            }
            LogTarget(parent);
        }

        private object GetDictValue(object instance, string currentSub)
        {
            try
            {
                if (instance is IDictionary dict)
                {
 
                    return dict[currentSub];
                    
                }
                else if (instance is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        var val = GetDictValue(item, currentSub);
                        if (val != null)
                            return val;
                    }
                }
            }
            catch { }
            return null;
        }

        private object GetMember(object parent, ref string name)
        {
            object member = null;

            if(parent == null)
            {
                member = AccessTools.Field(rootType, name);
                member ??= AccessTools.Property(rootType, name);
                member ??= AccessTools.Method(rootType, name);
            }
            else
            {
                member = AccessTools.Field(parent.GetType(), name);
                member ??= AccessTools.Property(parent.GetType(), name);
                member ??= AccessTools.Method(parent.GetType(), name);
            }
            name = "";

            return member;
        }

        private void SetCSValue(object setParent, object toSet, string toSetKey, object valueParent, object val, string name)
        {
            string valueString = "";
            object value = null;

            if (val is string str)
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(toSet is PropertyInfo prop ? prop.PropertyType : toSet is FieldInfo field ? field.FieldType : toSet as Type);
                value = typeConverter.ConvertFromString(str);
                valueString = str;
            }
            else if (val is PropertyInfo prop)
            {
                value = prop.GetValue(valueParent);
                valueString = prop.GetValue(valueParent).ToString();
            }
            else if (val is FieldInfo field)
            {
                value = field.GetValue(valueParent);
                valueString = field.GetValue(valueParent).ToString();
            }
            if (string.IsNullOrEmpty(toSetKey))
            {
                AccessTools.Method(toSet.GetType(), "SetValue", new Type[] { typeof(object), typeof(object) }).Invoke(toSet, new object[] { setParent, value });
            }
            else
            {
                SetDictValue(setParent, toSetKey, value);
            }

            SMonitor.Log($"Set {name} to {valueString}");

        }

        private void SetDictValue(object instance, string key, object value)
        {
            if (instance is IDictionary dict)
            {
                dict[key] = value;
                SMonitor.Log($"Set {key} to {value}");
            }
            else if (instance is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    SetDictValue(item, key, value);
                }
            }
        }

        private Type GetDictType(object instance)
        {
            if (instance is IDictionary dict)
            {
                return dict.GetType().GetGenericArguments()[1];
            }
            else if (instance is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var type = GetDictType(item);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        private void LogTarget(object instance)
        {
            if (instance?.ToString() is null)
            {
                SMonitor.Log("null");
                return;
            }
            if (instance is string s)
            {
                SMonitor.Log(s);
            }
            else if (instance is IDictionary dict)
            {
                foreach (var item in dict.Keys)
                {
                    LogTarget(new KeyValuePair<object, object>(item, dict[item]));
                }
            }
            else if (instance is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    LogTarget(item);
                }
            }
            else
            {
                SMonitor.Log(instance.ToString(), StardewModdingAPI.LogLevel.Info);
            }
        }

    }
}