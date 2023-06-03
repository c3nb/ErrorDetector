using HarmonyLib;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace ErrorDetector
{
    public static class Patches
    {
        [HarmonyPatch(typeof(ModEntry), "Active", MethodType.Setter)]
        public static class LoadPatch
        {
            public static void Postfix(ModEntry __instance)
            {
                Main.UpdateActives(__instance.ErrorOnLoading ? __instance : null);
            }
        }
        [HarmonyPatch(typeof(UnityModManager), "_Start")]
        public static class StartPatch
        {
            public static void Postfix()
            {
                Main.UpdateActives(null);
                Main.MoveFirst();
            }
        }
    }
}
