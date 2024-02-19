using Empire_Rewritten.Player;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.AI
{
    public class AIExpansionManager : AIModule
    {
        private static Dictionary<int, float> _tileAttractiveness = new Dictionary<int, float>();
        public AIExpansionManager(AIPlayer player) : base(player) { }

        public override void DoModuleAction()
        {
            // Choose a resource to increase production of.
            Dictionary<ResourceDef, float> prios = _player.GetResourcePriority();

            // This is pretty expensive, so we cache it. Technically getresourcepriority also calls this, but
            //   optimization is annoying so I'll leave it for now.
            Dictionary<ResourceDef, float> resourceChange = _player.Faction.GetResourceChange();

            ResourceDef target = prios.Keys.RandomElementByWeight(rd => prios[rd]);

            // We will try first to upgrade an existing facility, then build a new one at an existing settlement,
            //   then upgrading an existing settlement, and finally settle a new settlement with that resource.

            // 1. Upgrade an existing facility: Find the best facility that produces the most of the target resource
            //    and has an upkeep that we can pay while still having a surplus.
            Facility best = null;
            float bestProduction = 0;
            foreach (Settlement settlement in _player.Faction.GetTrackedSettlements())
            {
                SettlementDetails details = settlement.GetDetails();
                foreach (Facility facility in details.GetFacilities())
                {
                    if (!details.CanUpgrade(facility.def, out string tmp)) continue;

                    // Find production of this facility for the target resource, if it's not null.
                    float production = facility.GetProduction().Find(rm => rm.def == target)?.TotalModifier() ?? 0;
                    if (production <= 0) continue;

                    // Check if we can afford the upkeep.
                    bool canAfford = true;
                    foreach (ResourceModifier upkeep in facility.GetUpkeep())
                    {

                        float upkeepCost = upkeep.TotalModifier();
                        float upkeepChange = resourceChange[target];

                        if (-upkeepCost < upkeepChange)
                        {
                            canAfford = false;
                            break;
                        }

                    }

                    if (production > bestProduction && canAfford)
                    {
                        best = facility;
                        bestProduction = production;
                    }
                }
            }
            if (best != null)
            {
                // We found a facility to upgrade. Upgrade it.
                best.Settlement.GetDetails().UpgradeFacility(best.def);
                return;
            }

            // 2. Build a new facility: Find the best settlement that has a potential facility that produces the
            //    target resource, with an affordable upkeep.
            Settlement bestSettlement = null;
            FacilityDef bestFacility = null;
            float bestSettlementProduction = 0;
            foreach (Settlement settlement in _player.Faction.GetTrackedSettlements())
            {
                SettlementDetails details = settlement.GetDetails();
                // Go through all facilities
                foreach (FacilityDef facilityDef in DefDatabase<FacilityDef>.AllDefs)
                {
                    if (!details.CanBuild(facilityDef, out string tmp)) continue;

                    // Check if this facility produces the target resource.
                    ResourceModifier mod = facilityDef.production.Find(rm => rm.def == target);
                    if (mod == null) continue;

                    // Include the settlement's production multiplier, as that will be applied to the facility.
                    mod = mod.MergeWith(new ResourceModifier(target, 0, details.GetProductionMultiplierFor(target)));
                    float production = mod.TotalModifier();

                    // Check if we can afford the upkeep.
                    bool canAfford = true;
                    foreach (ResourceModifier upkeep in facilityDef.upkeep)
                    {

                        float upkeepCost = upkeep.TotalModifier();
                        float upkeepChange = resourceChange[target];

                        if (-upkeepCost < upkeepChange)
                        {
                            canAfford = false;
                            break;
                        }

                    }

                    if (production > bestSettlementProduction && canAfford)
                    {
                        bestSettlement = settlement;
                        bestFacility = facilityDef;
                        bestSettlementProduction = production;
                    }
                }
            }
            if (bestSettlement != null)
            {
                // We found a settlement to build a facility at. Build it.
                bestSettlement.GetDetails().BuildFacility(bestFacility);
                return;
            }

            // 3. Upgrade an existing settlement: We will upgrade a random settlement weighted towards
            //   settlements with lowest level. Bonus weight given to settlements with no free slots.
            Settlement bestS = _player.Faction.GetTrackedSettlements().RandomElementByWeight(settlement =>
            {
                SettlementDetails details = settlement.GetDetails();
                float weight = 1f / (2f * details.Level + 1);
                if (details.FacilitySlotsRemaining() == 0) weight *= 4;
                if (details.FacilitySlotsRemaining() == 1) weight *= 2;

                // We divide here in case we can't level ANY settlements; we want to have a non-zero sum of weights.
                if (!details.CanLevelSettlement()) weight /= 10000;
                return weight;
            });

            // The higher the level, the more likely we are to not upgrade.
            if (bestS != null && Rand.Value < 1f / bestS.GetDetails().Level + 0.4f && bestS.GetDetails().CanLevelSettlement())
            {
                bestS.GetDetails().LevelSettlement();
                return;
            }

            // 4. Settle a new settlement: Find the best tile to settle on, and settle it.
            int bestTile = _FindBestTile();
            if (bestTile != -1)
            {
                // Check if we can afford the settlement cost.
                Dictionary<ResourceDef, int> cost = SettlementDetails.SettlementCost;
                foreach (ResourceDef rd in cost.Keys)
                {
                    if (cost[rd] > _player.Faction.GetStockpile()[rd]) return;
                }

                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.Tile = bestTile;
                settlement.SetFaction(_player.Faction);

                // Generate a name
                List<string> used = new List<string>();
                List<Settlement> worldSettlements = Find.WorldObjects.Settlements;
                foreach (Settlement found in worldSettlements)
                {
                    used.Add(found.Name);
                }
                settlement.Name = NameGenerator.GenerateName(_player.Faction.def.factionNameMaker, used, true);

                EmpireWorldComp.Current.AddSettlement(settlement);
            }

            // Unfortunately, we're too poor to do anything. We wait for the next turn.
        }

        private int _FindBestTile()
        {
            // Find the best tile to settle on. We can only settle on our territory.
            int bestTile = -1;
            float bestAttractiveness = 0;
            foreach (Settlement settlement in _player.Faction.GetTrackedSettlements())
            {
                foreach (int tile in settlement.GetTerritory())
                {
                    float baseAttractiveness = _GetAttractiveness(tile);

                    // Add a bonus for being far from other settlements, including our own.
                    float distanceBonus = 1;
                    foreach (Settlement other in Find.WorldObjects.Settlements)
                    {
                        float distance = Find.WorldGrid.ApproxDistanceInTiles(tile, other.Tile);
                        if (distance > 15f) continue;

                        // We really don't want to settle close to our own settlements, even if it means
                        //   settling close to other factions.
                        float divisor = other.Faction == _player.Faction ? 25f : 50f;
                        distanceBonus += distance / divisor;
                    }

                    // TODO Add a bonus for having high resource production for resources we have low production of.

                    float attractiveness = baseAttractiveness * distanceBonus;
                    if (attractiveness > bestAttractiveness && TileFinder.IsValidTileForNewSettlement(tile))
                    {
                        bestTile = tile;
                        bestAttractiveness = attractiveness;
                    }
                }
            }

            return bestTile;
        }

        private static float _GetAttractiveness(int id)
        {
            if (!_tileAttractiveness.ContainsKey(id))
            {
                _tileAttractiveness.Add(id, _CalculateAttractiveness(id));
            }

            return _tileAttractiveness[id];
        }

        private static float _CalculateAttractiveness(int id)
        {
            Tile tile = Find.WorldGrid[id];

            float resourceWeight = _CalculateResourceWeight(tile);

            // Follow settlement selection weight for more compatibility with other mods.
            float biomeWeight = tile.biome.settlementSelectionWeight;

            // Hills are hard to build on.
            float hillWeight = (float)tile.hilliness + 1f;

            // Existing life is a good sign. Animals seems to go up to ~6.5x, so we scale it down slightly, favoring plants.
            float lifeWeight = tile.biome.animalDensity / 8f + tile.biome.plantDensity + 0.1f;

            // Rivers are nicer, but not required.
            float riverWeight = tile.Rivers?.Count ?? 0;
            if (riverWeight > 0) riverWeight = 1;
            riverWeight += 0.5f;

            return resourceWeight * biomeWeight * hillWeight * lifeWeight * riverWeight;
        }

        private static float _CalculateResourceWeight(Tile tile)
        {
            float multSum = 0;
            int count = 0;
            foreach (ResourceDef rd in DefDatabase<ResourceDef>.AllDefs)
            {
                multSum += rd.GetTileModifier(tile).TotalModifier();
                count++;
            }
            return multSum / count;
        }
    }
}