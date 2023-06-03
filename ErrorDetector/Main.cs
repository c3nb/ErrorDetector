using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;
using SFB;
using System.Text;
using System.IO;
using System;

namespace ErrorDetector
{
    public static class Main
    {
        public static ModEntry Mod { get; private set; }
        public static ModEntry.ModLogger Logger { get; private set; }  
        public static List<ModEntry> ActiveMods { get; private set; }
        public static Harmony Harmony { get; private set; }
        public static void Load(ModEntry modEntry)
        {
            Mod = modEntry;
            Logger = modEntry.Logger;
            Application.logMessageReceived += UnityLogCallback;
            Harmony = new Harmony(modEntry.Info.Id);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnGUI = m =>
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Export Errors"))
                    ExportErrors();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                for (int i = 0; i < Errors.Count; i++) 
                    Errors[i].Display();
            };
        }
        private static void UnityLogCallback(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception) 
                if (!ErrorHashes.Contains(condition + stackTrace))
                {
                    var err = new Error(new UnityLogException(condition, stackTrace));
                    Errors.Add(err);
                    ErrorHashes.Add(err.Exception.Hash);
                }
        }
        public static readonly List<Error> Errors = new List<Error>();
        public static readonly HashSet<string> ErrorHashes = new HashSet<string>();
        public static void UpdateActives(ModEntry with)
        {
            ActiveMods = modEntries.Where(m => m.Active).ToList();
            if (with != null) ActiveMods.Add(with);
        }
        public static void MoveFirst()
        {
            int index = modEntries.IndexOf(Mod);
            modEntries.RemoveAt(index);
            modEntries.Insert(0, Mod);
        }
        public static void ExportErrors()
        {
            var path = StandaloneFileBrowser.OpenFolderPanel("Save Directory", "", false);
            if (path.Length == 0) return;
            StringBuilder sb = new StringBuilder();
            foreach (Error err in Errors)
                sb.AppendLine(err.ToString());
            File.WriteAllText(Path.Combine(path[0], "Errors.txt"), sb.ToString());
        }
    }
}
