using Empire_Rewritten.Settlements;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.HarmonyPatches
{

    [HarmonyPatch(typeof(SettleInEmptyTileUtility), "Settle")]
    public static class SettleEmptyTilePatch
    {

        [HarmonyPostfix]
        public static void SettlePatch()
        {
            // When the player settles a new tile (from a caravan), add a settlement manager for it and claim it.
            foreach (Settlement settlement in Find.WorldObjects.Settlements)
            {
                if (settlement.Faction.IsPlayer)
                {
                    // If it's not tracked, this is the new settlement. Add a settlement manager for it and claim it.
                    if (EmpireWorldComp.Current.GetTrackedFactions().Contains(settlement.Faction) &&
                        !EmpireWorldComp.Current.GetTrackedSettlements().Contains(settlement))
                    {
                        EmpireWorldComp.Current.SettlementDetails.Add(settlement, new SettlementDetails(settlement));
                        EmpireWorldComp.Current.TerritoryManager.AddClaims(settlement);
                    }
                }
            }
        }
    }
}
