using Empire_Rewritten.Military;
using Empire_Rewritten.Settlements;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.HarmonyPatches
{

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class PawnPatch
    {

        [HarmonyPostfix]
        public static void KillPatch(Pawn __instance)
        {
            // If pawn dies anywhere, remove it from tracker.
            MilitaryManager manager = null;
            if (manager.AllPawns.Contains(__instance))
            {
                manager?.RemovePawn(__instance);
            }
        }
    }
}
