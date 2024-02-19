using Empire_Rewritten.Processes;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten
{
    public static class DebugActions
    {
        [PublicAPI]
        [DebugAction("Empire", "Log Settlement Info", allowedGameStates = AllowedGameStates.Playing)]
        public static void LogSettlementInfo()
        {
            foreach (RimWorld.Planet.Settlement s in Find.WorldObjects.Settlements)
            {
                if (s.GetDetails() != null)
                {
                    Logger.Message($"{s.Name} ({s.GetDetails().Level}):");
                    foreach (Facility f in s.GetDetails().GetFacilities())
                    {
                        Logger.Message($"{f.def.label} ({f.Size})");
                    }
                }
            }
        }

        [PublicAPI]
        [DebugAction("Empire", "Log Stockpiles", allowedGameStates = AllowedGameStates.Playing)]
        public static void LogStockpiles()
        {
            foreach (Faction f in EmpireWorldComp.Current.GetTrackedFactions())
            {
                Logger.Message($"-------- {f.Name} --------");
                Dictionary<ResourceDef, float> val = f.GetStockpile();
                Dictionary<ResourceDef, float> cng = f.GetResourceChange();
                foreach (ResourceDef r in val.Keys)
                {
                    if (cng.ContainsKey(r)) Logger.Message($"{r.label}: {val[r]} +/- {cng[r]}");
                    else Logger.Message($"{r.label}: {val[r]} +/- 0");

                }
                Logger.Message("");
            }
        }

        [PublicAPI]
        [DebugAction("Empire", "Log Factions", allowedGameStates = AllowedGameStates.Playing)]
        public static void LogFactions()
        {
            foreach (Faction f in EmpireWorldComp.Current.GetTrackedFactions())
            {
                Logger.Message($"Found {f.Name}.");
            }
        }

        [PublicAPI]
        [DebugAction("Empire", "Log Settlement Processes", allowedGameStates = AllowedGameStates.Playing)]
        public static void LogSettlementProcesses()
        {
            foreach (SettlementDetails s in EmpireWorldComp.Current.SettlementDetails.Values)
            {
                Logger.Message($"-------- {s.Settlement.Name} ({s.Settlement.Faction.Name}) --------");
                foreach (Process p in s.GetProcesses())
                {
                    Logger.Message($"{p.Label}: {p.Tooltip} ({p.ProgressPct})");
                }
            }
        }

        [PublicAPI]
        [DebugAction("Empire", "Give Player Resources", allowedGameStates = AllowedGameStates.Playing)]
        public static void GivePlayerResources()
        {
            if (EmpireWorldComp.Current.GetTrackedFactions().Contains(Faction.OfPlayer))
            {
                for (int i = 0; i < 50; i++)
                {
                    ResourceDef rnd = DefDatabase<ResourceDef>.AllDefsListForReading.RandomElement();
                    if (Faction.OfPlayer.GetStockpile().ContainsKey(rnd))
                    {
                        Faction.OfPlayer.GetStockpile()[rnd] += 100;
                    }
                    else
                    {
                        Faction.OfPlayer.GetStockpile().Add(rnd, 100);
                    }
                }

            }
        }

        [PublicAPI]
        [DebugAction("Empire", "Run Tax", allowedGameStates = AllowedGameStates.Playing)]
        public static void RunTax()
        {
            if (EmpireWorldComp.Current.GetTrackedFactions().Contains(Faction.OfPlayer))
            {
                EmpireWorldComp.Current._DebugDoTax();
            }
        }

        [PublicAPI]
        [DebugAction("Empire", "Log Tax Entries", allowedGameStates = AllowedGameStates.Playing)]
        public static void LogTaxEntries()
        {
            if (EmpireWorldComp.Current.GetTrackedFactions().Contains(Faction.OfPlayer))
            {
                Dictionary<ResourceDef, EmpireWorldComp.TaxList> l = EmpireWorldComp.Current.TaxDetails;
                foreach (ResourceDef r in l.Keys)
                {
                    Logger.Message($"{r.label}:");
                    foreach (Thing t in l[r].Things)
                    {
                        Logger.Message($"  {t.stackCount} x {t.Label}");
                    }
                }
            }
        }
    }
}