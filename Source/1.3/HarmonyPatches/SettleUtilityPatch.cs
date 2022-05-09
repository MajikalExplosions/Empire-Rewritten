using Empire_Rewritten.Controllers;
using RimWorld;
using RimWorld.Planet;

namespace Empire_Rewritten.HarmonyPatches
{
    public static class SettleUtilityPatch
    {
        public static void Postfix(Faction faction, Settlement __result)
        {
            if (faction != Faction.OfPlayer || __result == null)
            {
                return;
            }

            UpdateController.CurrentWorldInstance?.FactionController?.GetOwnedEmpire(faction)?.AddSettlement(__result);
        }
    }
}
