using Empire_Rewritten.Utils;
using HarmonyLib;

namespace Empire_Rewritten.HarmonyPatches
{
    public static class HarmonyPatcher
    {
        /// <summary>
        ///     Runs our harmony patches.
        /// </summary>
        public static void DoPatches()
        {
            Harmony harmony = new Harmony("EmpireRewritten.HarmonyPatches");
            harmony.PatchAll();

            Logger.Log("Patches completed!");
        }
    }
}
