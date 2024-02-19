using Empire_Rewritten.Processes;
using Empire_Rewritten.Resources;
using Empire_Rewritten.UI;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Logger = Empire_Rewritten.Utils.Logger;

namespace Empire_Rewritten.Settlements
{
    /// <summary>
    ///     Manages a single <see cref="Settlement"/> and all its <see cref="Facility">Facilities</see>.
    ///     It also manages the <see cref="ResourceModifier">ResourceModifiers</see> from each <see cref="Facility" />.
    /// </summary>
    public class SettlementDetails : IExposable
    {

        // TODO Balance the settlement cost
        public static readonly Dictionary<ResourceDef, int> SettlementCost = new Dictionary<ResourceDef, int>
        {
            { EmpireDefOf.Empire_Res_Wood, 1000 }, { EmpireDefOf.Empire_Res_Stone, 500 }
        };
        public const int MaxLevel = 10;

        private Settlement _settlement;
        public Settlement Settlement
        {
            get => _settlement;
            private set => _settlement = value;
        }

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                _level = Math.Max(0, Math.Min(value, MaxLevel));

                // Update territory
                if (EmpireWorldComp.Current.TerritoryManager != null && EmpireWorldComp.Current.SettlementDetails.ContainsKey(Settlement))
                {
                    EmpireWorldComp.Current.TerritoryManager.AddClaims(Settlement);
                }

                // Remove process if it exists.
                SettlementLevelProcess process = _processes.FirstOrDefault(p => p is SettlementLevelProcess) as SettlementLevelProcess;
                if (process != null && process.Progress >= 1) _processes.Remove(process);
            }
        }

        private readonly List<Gizmo> _gizmos = new List<Gizmo>();

        private List<Facility> _facilities = new List<Facility>();

        private List<Process> _processes = new List<Process>();

        public SettlementDetails(Settlement settlement) : base()
        {
            _settlement = settlement;
            Level = 1;
        }

        /// <summary>
        ///     Used for Loading/Saving
        /// </summary>
        [UsedImplicitly]
        public SettlementDetails() { }

        public bool RemoveProcess(Process process)
        {
            if (_processes.Contains(process))
            {
                _processes.Remove(process);
                return true;
            }
            return false;
        }

        public bool BuildFacility(FacilityDef facilityDef, bool instant = false)
        {
            if (!CanBuild(facilityDef, out string tmp)) return false;

            if (instant)
            {
                _facilities.Add(new Facility(facilityDef, Settlement));
                ChangeFacilitySize(facilityDef, 1);
                return true;
            }


            foreach (FacilityDef.BuildCostEntry cost in facilityDef.buildCost)
            {
                Settlement.Faction.GetStockpile()[cost.resource] -= cost.amount;
            }
            _processes.Add(new FacilityBuildProcess(Settlement, facilityDef));

            // FacilityBuildProcess adds one to the facility size, so we make sure it starts at 0.
            Facility f = new Facility(facilityDef, Settlement);
            _facilities.Add(f);

            return true;
        }

        public bool DemolishFacility(FacilityDef facilityDef)
        {
            if (!HasFacility(facilityDef)) return false;
            Facility fac = _facilities.FirstOrDefault(f => f.def == facilityDef);
            if (fac == null) return false;

            _facilities.Remove(fac);
            // Remove any processes that are building this facility.
            FacilityBuildProcess process = _processes.FirstOrDefault(p => p is FacilityBuildProcess && (p as FacilityBuildProcess).FacilityDef == facilityDef) as FacilityBuildProcess;
            process?.Cancel();
            _processes.Remove(process);
            return true;
        }

        public bool UpgradeFacility(FacilityDef facilityDef)
        {
            if (!CanUpgrade(facilityDef, out string tmp)) return false;

            foreach (FacilityDef.BuildCostEntry cost in facilityDef.buildCost)
            {
                Settlement.Faction.GetStockpile()[cost.resource] -= cost.amount;
            }
            _processes.Add(new FacilityBuildProcess(Settlement, facilityDef));

            return true;
        }

        public bool CancelFacility(FacilityDef facilityDef, bool refund = false)
        {
            if (!HasFacility(facilityDef)) return false;
            FacilityBuildProcess process = _processes.FirstOrDefault(p => p is FacilityBuildProcess && (p as FacilityBuildProcess).FacilityDef == facilityDef) as FacilityBuildProcess;
            if (process == null) return false;
            process.Cancel();
            _processes.Remove(process);

            // Check if it's level 0, if so, remove it.
            Facility facility = _facilities.FirstOrDefault(f => f.def == facilityDef);
            if (facility.Size == 0)
            {
                _facilities.Remove(facility);
            }

            // Refund resources if refund is true.
            if (refund)
            {
                foreach (FacilityDef.BuildCostEntry cost in facilityDef.buildCost)
                {
                    Settlement.Faction.GetStockpile()[cost.resource] += cost.amount;
                }
            }
            return true;
        }

        public bool CancelLevel(bool refund = false)
        {
            SettlementLevelProcess process = _processes.FirstOrDefault(p => p is SettlementLevelProcess) as SettlementLevelProcess;
            if (process == null) return false;
            process.Cancel();
            _processes.Remove(process);

            if (refund)
            {
                foreach (KeyValuePair<ResourceDef, int> pair in SettlementCost)
                {
                    Settlement.Faction.GetStockpile()[pair.Key] += pair.Value;
                }
            }

            return true;
        }

        public bool LevelSettlement()
        {
            if (!CanLevelSettlement()) return false;

            foreach (KeyValuePair<ResourceDef, int> pair in SettlementCost)
            {
                Settlement.Faction.GetStockpile()[pair.Key] -= pair.Value;
            }
            _processes.Add(new SettlementLevelProcess(Settlement));

            return true;
        }

        public void ChangeFacilitySize(FacilityDef def, int delta)
        {
            if (def == null)
            {
                Logger.Error("Tried to change the size of a null facility");
                return;
            }
            if (!HasFacility(def))
            {
                Logger.Error("Tried to change the size of facility that doesn't exist.");
                return;
            }

            // Change size of facility with specific def by delta.
            _facilities.FirstOrDefault(f => f.def == def).ChangeSize(delta);

            // Check processes for FacilityBuildProcess and remove if it's finished.
            FacilityBuildProcess process = _processes.FirstOrDefault(p => p is FacilityBuildProcess && (p as FacilityBuildProcess).FacilityDef == def) as FacilityBuildProcess;
            if (process != null && process.Progress >= 1) _processes.Remove(process);
        }

        public bool CanBuild(FacilityDef facilityDef, out string reason)
        {
            // If facilityDef is null, settlement has max facilities, or facility already exists, return false.
            if (facilityDef == null)
            {
                reason = "Empire_SD_ReasonNull".Translate();
                return false;
            }
            if (_facilities.Count >= Level)
            {
                reason = "Empire_SD_ReasonMaxFacilities".Translate();
                return false;
            }
            if (HasFacility(facilityDef))
            {
                reason = "Empire_SD_ReasonAlreadyExists".Translate();
                return false;
            }

            // If player, check for exact research unlock. If AI, check for tech level. If null, assume no research required.
            if (facilityDef.requiredResearch != null)
            {
                reason = "Empire_SD_ReasonNoResearch".Translate();

                if ((!Settlement.Faction.GetPlayer().IsAI()) && Find.ResearchManager.GetProgress(facilityDef.requiredResearch) < 1)
                {
                    return false;
                }
                if (Settlement.Faction.GetPlayer().IsAI() && Settlement.Faction.def.techLevel < facilityDef.requiredResearch.techLevel)
                {
                    return false;
                }
            }

            if (!_CanAfford(facilityDef))
            {
                reason = "Empire_SD_ReasonTooExpensive".Translate();
                return false;
            }

            reason = "";
            return true;
        }

        public float UpgradeProgress(FacilityDef facilityDef)
        {
            if (!HasFacility(facilityDef)) return 0;
            FacilityBuildProcess process = _processes.FirstOrDefault(p => p is FacilityBuildProcess && (p as FacilityBuildProcess).FacilityDef == facilityDef) as FacilityBuildProcess;
            if (process == null) return -1;
            return process.ProgressPct;
        }

        public float LevelProgress()
        {
            SettlementLevelProcess process = _processes.FirstOrDefault(p => p is SettlementLevelProcess) as SettlementLevelProcess;
            if (process == null) return -1;
            return process.ProgressPct;
        }

        public bool CanUpgrade(FacilityDef facilityDef, out string reason)
        {
            if (!HasFacility(facilityDef))
            {
                reason = "Empire_SD_ReasonNoFacility".Translate();
                return false;
            }

            // If we're at max level, return false. Also, the max size is x^2/4 + x, where x is the level;
            //   this rewards larger settlements and counteracts having many small settlements with 1 building
            if (_facilities.Where(f => f.def == facilityDef).Sum(f => f.Size) >= GetMaxFacilitySize())
            {
                reason = "Empire_SD_ReasonMaxSize".Translate();
                return false;
            }

            // If it's already being upgraded, return false.
            if (_processes.Any(p => p is FacilityBuildProcess process && process.FacilityDef == facilityDef))
            {
                reason = "Empire_SD_ReasonAlreadyUpgrading".Translate();
                return false;
            }

            if (! _CanAfford(facilityDef))
            {
                reason = "Empire_SD_ReasonTooExpensive".Translate();
                return false;
            }

            reason = "";
            return true;
        }

        public bool CanLevelSettlement()
        {
            if (Level >= MaxLevel) return false;

            if (_processes.Any(p => p is SettlementLevelProcess)) return false;

            return _CanAffordLevel();
        }

        public int GetMaxFacilitySize()
        {
            return Mathf.CeilToInt(Level * Level / 4 + Level);
        }

        public int FacilitySlotsRemaining()
        {
            return Level - _facilities.Count;
        }

        public int GetTerritoryRadius()
        {
            return (int)Math.Ceiling(Math.Sqrt(Level * 3 + (int)Settlement.Faction.def.techLevel * 2 - 4));
        }


        private bool _CanAfford(FacilityDef facilityDef)
        {
            if (facilityDef.buildCost == null) return true;

            // Check if faction has enough resources to build facility.
            Dictionary<ResourceDef, float> resources = Settlement.Faction.GetStockpile();

            foreach (FacilityDef.BuildCostEntry cost in facilityDef.buildCost)
            {
                if (!resources.ContainsKey(cost.resource)) return false;
                if (resources[cost.resource] < cost.amount) return false;
            }
            return true;
        }

        private bool _CanAffordLevel()
        {
            foreach (KeyValuePair<ResourceDef, int> pair in SettlementCost)
            {
                if (Settlement.Faction.GetStockpile().TryGetValue(pair.Key, out float amount))
                {
                    if (amount < pair.Value) return false;
                }
                else return false;
            }

            return true;
        }

        public bool HasFacility(FacilityDef facilityDef)
        {
            return _facilities.Any(f => f.def == facilityDef);
        }

        public Facility GetFacility(FacilityDef facilityDef)
        {
            return _facilities.FirstOrDefault(f => f.def == facilityDef);
        }

        public List<Facility> GetFacilities()
        {
            return _facilities;
        }

        public List<Process> GetProcesses()
        {
            return _processes;
        }

        public float GetProductionMultiplierFor(ResourceDef def)
        {
            float mult = 1;
            foreach (Facility facility in _facilities)
            {
                // Go through each facility and add its modifiers to the total modifier.
                foreach (ResourceModifier facilityModifier in facility.def.production)
                {
                    if (facilityModifier.def == def)
                    {
                        mult *= facilityModifier.multiplier;
                    }
                }
            }

            // Add tile modifiers.
            mult *= def.GetTileModifier(Find.WorldGrid.tiles[Settlement.Tile]).multiplier;

            return mult;
        }

        public float GetUpkeepMultiplierFor(ResourceDef def)
        {
            float mult = 1;
            foreach (Facility facility in _facilities)
            {
                // Go through each facility and add its modifiers to the total modifier.
                foreach (ResourceModifier facilityModifier in facility.def.upkeep)
                {
                    if (facilityModifier.def == def)
                    {
                        mult *= facilityModifier.multiplier;
                    }
                }
            }

            // Upkeep isn't affected by tile modifiers.

            return mult;
        }

        public List<ResourceModifier> GetProduction()
        {
            // TODO Optimize; every facility calls GetProductionMultiplierFor(), which then calls every facility again.
            //   So, this is O(n^2) when it could be O(n). Most of the time n is small, so we let it slide for now.
            Dictionary<ResourceDef, ResourceModifier> production = new Dictionary<ResourceDef, ResourceModifier>();
            foreach (Facility facility in _facilities)
            {
                foreach (ResourceModifier modifier in facility.GetProduction())
                {
                    if (!production.ContainsKey(modifier.def)) production.Add(modifier.def, new ResourceModifier(modifier.def, 0, GetProductionMultiplierFor(modifier.def)));
                    production[modifier.def].offset += modifier.offset;

                }
            }

            // The settlement's tile may also have a base production (offset) so we add that.
            foreach (ResourceDef def in production.Keys)
            {
                production[def].offset += def.GetTileModifier(Find.WorldGrid.tiles[Settlement.Tile]).offset;
            }

            return production.Values.ToList();
        }

        public List<ResourceModifier> GetUpkeep()
        {
            Dictionary<ResourceDef, ResourceModifier> upkeep = new Dictionary<ResourceDef, ResourceModifier>();
            foreach (Facility facility in _facilities)
            {
                foreach (ResourceModifier modifier in facility.GetUpkeep())
                {
                    if (!upkeep.ContainsKey(modifier.def)) upkeep.Add(modifier.def, new ResourceModifier(modifier.def, 0, GetUpkeepMultiplierFor(modifier.def)));
                    upkeep[modifier.def].offset += modifier.offset;
                }
            }

            // The settlement's tile may also have a base production (offset) so we add that.
            foreach (ResourceDef def in upkeep.Keys)
            {
                upkeep[def].offset += def.GetTileModifier(Find.WorldGrid.tiles[Settlement.Tile]).offset;
            }

            return upkeep.Values.ToList();
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Facility facility in _facilities)
            {
                IEnumerable<Gizmo> facilityGizmos = facility.def.FacilityWorker?.GetGizmos();
                if (facilityGizmos != null)
                {
                    foreach (Gizmo gizmo in facilityGizmos)
                    {
                        yield return gizmo;
                    }
                }
            }
            // We can only view the settlement details if it's our own settlement, or if we're in dev mode.
            if (Settlement.Faction.IsPlayer || Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Empire_SM_ViewDetail".Translate(),
                    defaultDesc = "Empire_SM_ViewDetailDesc".Translate(),
                    icon = Settlement.Faction.def.FactionIcon,// TODO Change to actual info icon
                    action = () => Find.WindowStack.Add(new SettlementDetailWindow(Settlement))
                };
            }
        }

        public static bool CanBuildAt(int tile, out string reason)
        {
            Tile t = Find.WorldGrid[tile];
            if (t.WaterCovered)
            {
                reason = "Empire_SM_ErrBuildOnWater".Translate();
                return false;
            }
            if (t.biome.impassable || t.hilliness == Hilliness.Impassable)
            {
                reason = "Empire_SM_ErrBuildOnImpassable".Translate();
                return false;
            }

            reason = "";
            return true;
        }

        public static bool CanBuildAt(Faction faction, int tile, out string reason)
        {
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile))
            {
                reason = "Empire_SM_ErrBuildTooClose".Translate();
                return false;
            }
            if (EmpireWorldComp.Current.TerritoryManager.GetTileOwner(tile) == null || EmpireWorldComp.Current.TerritoryManager.GetTileOwner(tile).Faction != faction)
            {
                reason = "Empire_SM_ErrBuildTileUnowned".Translate();
                return false;
            }
            foreach (ResourceDef def in SettlementCost.Keys)
            {
                if (faction.GetStockpile().TryGetValue(def, out float amount))
                {
                    if (amount < SettlementCost[def])
                    {
                        reason = "Empire_SM_ErrBuildNotEnoughResources".Translate();
                        return false;
                    }
                }
                else
                {
                    reason = "Empire_SM_ErrBuildNotEnoughResources".Translate();
                    return false;
                }
            }

            return CanBuildAt(tile, out reason);
        }

        public void ExposeData()
        {
            // DONE!
            Scribe_References.Look(ref _settlement, "settlement");
            Scribe_Values.Look(ref _level, "level");

            Scribe_Collections.Look(ref _facilities, "facilities", LookMode.Deep);
            Scribe_Collections.Look(ref _processes, "processes", LookMode.Deep);
        }
    }
}