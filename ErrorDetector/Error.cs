using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace ErrorDetector
{
    public class Error
    {
        public readonly UnityLogException Exception;
        public readonly bool ModError;
        public readonly bool ModContains;
        public readonly IEnumerable<UnityModManager.ModEntry> Mods;
        public readonly List<Call> Calls;
        bool expanded = false;
        bool callstackExpanded = false;
        public Error(UnityLogException ule)
        {
            Exception = ule;
            Calls = GetCalls(ule);
            Mods = Calls.Select(c => c.Mod).Where(e => e != null).Distinct();
            ModError = Calls[0].Mod != null;
            ModContains = ModError | Mods.Any();
        }
        public void Display()
        {
            if (expanded = GUILayout.Toggle(expanded, $"Error From <b>{(ModError ? "Mod" : "Internal")}</b>"))
            {
                BeginIndent();
                if (ModContains)
                    GUILayout.Label($"Related Mods: {ToString(Mods, e => e.Info.DisplayName)}");
                GUILayout.Label($"Error Type: {Exception.ExceptionType}");
                GUILayout.Label($"Error Message: {Exception.Message}");
                GUILayout.Label($"Target Site: {Exception.TargetSite}");
                if (callstackExpanded = GUILayout.Toggle(callstackExpanded, "Call Stack"))
                {
                    BeginIndent();
                    for (int i = 0; i < Calls.Count; i++)
                        GUILayout.Label($"{(i == 0 ? "Target Site" : "at")} {Calls[i]}");
                    EndIndent();
                }
                EndIndent();
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Error From {(ModError ? "Mod" : "Internal")}");
            if (ModContains)
                sb.Append(' ', 4).AppendLine($"Related Mods: {ToString(Mods, e => e.Info.DisplayName)}");
            sb.Append(' ', 4).AppendLine($"Error Type: {Exception.ExceptionType}");
            sb.Append(' ', 4).AppendLine($"Error Message: {Exception.Message}");
            sb.Append(' ', 4).AppendLine($"Target Site: {Exception.TargetSite}");
            sb.Append(' ', 4).AppendLine("Call Stack");
            for (int i = 0; i < Calls.Count; i++)
                sb.Append(' ', 8).AppendLine($"{(i == 0 ? "Target Site" : "at")} {Calls[i]}");
            return sb.ToString();
        }
        public static List<Call> GetCalls(UnityLogException ule)
        {
            var calls = new List<Call>();
            for (int i = 0; i < ule.CallStack.Length; i++)
            {
                if (IsModAssembly(ule.CallStack[i].DeclaringType?.Assembly, out var entry))
                    calls.Add(new Call(entry, ule.CallStack[i]));
                else calls.Add(new Call(null, ule.CallStack[i]));
            }
            return calls;
        }
        public static bool IsModAssembly(Assembly ass, out UnityModManager.ModEntry entry)
        {
            entry = null;
            if (ass == null) return false;
            for (int i = 0; i < Main.ActiveMods.Count; i++)
            {
                entry = Main.ActiveMods[i];
                if (entry.Assembly.FullName == ass.FullName)
                    return true;
            }
            return false;
        }
        public static string FormatMethod(MethodBase method)
        {
            var args = method.GetParameters();
            var declType = method.DeclaringType;
            var argsString = args.Aggregate("", (c, n) => $"{c}{n.ParameterType.FullName} {n.Name}, ");
            if (argsString.Length > 2)
                argsString = argsString.Remove(argsString.Length - 2);
            return $"{declType.FullName}.{method.Name}({argsString})";
        }
        public static string ToString<T>(IEnumerable<T> enumerable, Func<T, string> toString = null)
        {
            var str = enumerable.Aggregate("", (c, n) => $"{c}{(toString != null ? toString(n) : n.ToString())}, ");
            if (str.Length > 2) str = str.Remove(str.Length - 2);
            return str;
        }
        public static void BeginIndent(float space = 20)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(space);
            GUILayout.BeginVertical();
        }
        public static void EndIndent()
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
