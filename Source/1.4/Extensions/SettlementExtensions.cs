using Empire_Rewritten.Settlements;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace Empire_Rewritten
{
    public static class SettlementExtensions
    {
        private static EmpireWorldComp _wc => EmpireWorldComp.Current;

        public static bool IsPlayerSettlement(this Settlement settlement)
        {
            return settlement.Faction == Faction.OfPlayer;
        }

        public static SettlementDetails GetDetails(this Settlement settlement)
        {
            // Return settlement manager if it exists, null if not.
            return _wc.SettlementDetails.TryGetValue(settlement, out SettlementDetails details) ? details : null;
        }

        public static List<int> GetTerritory(this Settlement settlement)
        {
            return _wc.TerritoryManager.GetTerritory(settlement);
        }

        public static bool IsPlayerMap(this Settlement settlement)
        {
            return settlement.Faction == Faction.OfPlayer && settlement.HasMap;
        }
    }
}