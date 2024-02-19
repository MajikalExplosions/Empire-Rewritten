using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.HarmonyPatches
{
    [HarmonyPatch(typeof(Settlement), "GetGizmos")]
    public static class SettlementPatch
    {

        [HarmonyPostfix]
        public static IEnumerable<Gizmo> GetGizmosPatch(IEnumerable<Gizmo> result, Settlement __instance)
        {
            foreach (Gizmo g in result) yield return g;

            if (__instance.Faction.GetTrackedSettlements().Contains(__instance))
            {
                foreach (Gizmo g in __instance.GetDetails().GetGizmos())
                {
                    yield return g;
                }
            }
        }
    }
}
