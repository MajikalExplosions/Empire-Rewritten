using Empire_Rewritten.Factions;
using Empire_Rewritten.Player;
using Empire_Rewritten.Resources;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Empire_Rewritten
{
    public static class FactionExtensions
    {
        private static EmpireWorldComp _wc => EmpireWorldComp.Current;

        public static FactionDetails GetDetails(this Faction faction)
        {
            // Return settlement manager if it exists, null if not.
            return _wc.FactionDetails.TryGetValue(faction, out FactionDetails details) ? details : null;
        }

        public static List<Settlement> GetTrackedSettlements(this Faction faction)
        {
            return _wc.GetTrackedSettlements().Where(x => x.Faction == faction).ToList();
        }

        public static List<Settlement> GetAllSettlements(this Faction faction)
        {
            return Find.WorldObjects.Settlements.Where(x => x.Faction == faction).ToList();
        }

        public static BasePlayer GetPlayer(this Faction faction)
        {
            return _wc.PlayerController.Players.ContainsKey(faction) ? _wc.PlayerController.Players[faction] : null;
        }

        public static Dictionary<ResourceDef, float> GetStockpile(this Faction faction)
        {
            return faction.GetDetails()?.Stockpile;
        }

        public static Emblem GetEmblem(this Faction faction)
        {
            return faction.GetDetails()?.Emblem;
        }

        public static Dictionary<ResourceDef, float> GetResourceChange(this Faction faction)
        {
            Dictionary<ResourceDef, float> production = new Dictionary<ResourceDef, float>();

            foreach (Settlement settlement in faction.GetTrackedSettlements())
            {
                // Compute total production of each settlement, and add it to the total.
                List<ResourceModifier> prod = settlement.GetDetails().GetProduction();
                List<ResourceModifier> upkeep = settlement.GetDetails().GetUpkeep();
                foreach (ResourceModifier rm in prod)
                {
                    if (!production.ContainsKey(rm.def)) production.Add(rm.def, 0);

                    production[rm.def] += rm.TotalModifier();
                }

                foreach (ResourceModifier rm in upkeep)
                {
                    if (!production.ContainsKey(rm.def)) production.Add(rm.def, 0);

                    production[rm.def] -= rm.TotalModifier();
                }
            }

            return production;
        }

        public static Dictionary<ResourceDef, float> GetAdjustedResourceChange(this Faction faction)
        {
            Dictionary<ResourceDef, float> change = faction.GetResourceChange();
            Dictionary<ResourceDef, float> stockpile = faction.GetStockpile();

            float deficit = 1;
            foreach (ResourceDef def in DefDatabase<ResourceDef>.AllDefs)
            {
                float c = change.ContainsKey(def) ? change[def] : 0;
                float s = stockpile.ContainsKey(def) ? stockpile[def] : 0;
                if (c + s < 0)
                {
                    // If we are losing 20, and only have 5 in the stockpile, deficit = 0.75.
                    // This means we need to reduce the change by 75% to avoid going negative.
                    deficit = Math.Max(deficit, 1 - s / -c);
                }
            }

            List<ResourceDef> k = change.Keys.ToList();
            foreach (ResourceDef def in k) change[def] *= deficit;

            return change;
        }
        // TODO Territory info?
    }
}