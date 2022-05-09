using System;
using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Facilities;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Territories;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Settlements
{
    /// <summary>
    ///     Manages settlements, and storage.
    /// </summary>
    public class Empire : IExposable, ILoadReferenceable
    {
        [NotNull] public static readonly Dictionary<ThingDef, int> SettlementCost = new Dictionary<ThingDef, int>
        {
            { ThingDefOf.WoodLog, 500 }, { ThingDefOf.Steel, 100 }, { ThingDefOf.ComponentIndustrial, 12 }, { ThingDefOf.Silver, 200 },
        };

        private bool isAIPlayer;
        private bool territoryIsDirty;

        [NotNull] private Dictionary<Settlement, FacilityManager> settlements = new Dictionary<Settlement, FacilityManager>();
        [NotNull] private Faction faction;
        private List<FacilityManager> facilityManagersForLoading = new List<FacilityManager>();

        private List<Settlement> settlementsForLoading = new List<Settlement>();

        [NotNull] private StorageTracker storageTracker = new StorageTracker();
        private Territory cachedTerritory;

        [UsedImplicitly]
        public Empire() { }

        public Empire([NotNull] Faction faction, bool isAIPlayer = true)
        {
            this.faction = faction ?? throw new ArgumentNullException(nameof(faction));
            this.isAIPlayer = isAIPlayer;
        }

        public bool IsAIPlayer => isAIPlayer;
        [NotNull] public Faction Faction => faction;

        [NotNull] public StorageTracker StorageTracker => storageTracker;
        [NotNull] public Dictionary<Settlement, FacilityManager> Settlements => settlements;
        [NotNull] public IEnumerable<int> SettlementTiles => Settlements.Keys.Select(settlement => settlement?.Tile ?? -1);

        [CanBeNull]
        private Territory Territory
        {
            get
            {
                if (cachedTerritory == null || territoryIsDirty)
                {
                    territoryIsDirty = false;
                    cachedTerritory = TerritoryManager.CurrentInstance?.GetTerritory(faction);
                }

                return cachedTerritory;
            }
        }

        [NotNull] [ItemNotNull] public IEnumerable<FacilityManager> AllFacilityManagers => settlements.Values;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref settlements,
                                    "settlements",
                                    LookMode.Reference,
                                    LookMode.Deep,
                                    ref settlementsForLoading,
                                    ref facilityManagersForLoading);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Deep.Look(ref storageTracker, "storageTracker");
            Scribe_Values.Look(ref isAIPlayer, "isAIPlayer");
        }

        [Pure]
        public string GetUniqueLoadID()
        {
            return $"{nameof(Empire)}_{GetHashCode()}";
        }

        public void SetTerritoryDirty()
        {
            territoryIsDirty = true;
        }

        /// <summary>
        ///     Compiles a complete dictionary of all the resources a faction is producing and their modifiers.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public Dictionary<ResourceDef, ResourceModifier> ResourceModifiersFromAllFacilities()
        {
            Dictionary<ResourceDef, ResourceModifier> resourceModifiers = new Dictionary<ResourceDef, ResourceModifier>();
            List<FacilityManager> facilities = settlements.Values.ToList();
            foreach (FacilityManager facilityManager in facilities)
            {
                if (facilityManager is null)
                {
                    Logger.Error($"{nameof(Empire)} {faction.Name} has a null {nameof(FacilityManager)}.");
                    continue;
                }

                foreach (ResourceModifier resourceModifier in facilityManager.Modifiers)
                {
                    if (resourceModifiers.ContainsKey(resourceModifier.def))
                    {
                        ResourceModifier newModifier = resourceModifiers[resourceModifier.def].MergeWithModifier(resourceModifier);
                        resourceModifiers[resourceModifier.def] = newModifier;
                    }
                    else
                    {
                        resourceModifiers.Add(resourceModifier.def, resourceModifier);
                    }
                }
            }

            return resourceModifiers;
        }

        /// <summary>
        ///     Place a settlement on a <paramref name="tile" />.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>If the settlement was built.</returns>
        public bool BuildNewSettlementOnTile(Tile tile)
        {
            if (tile != null)
            {
                int tileId = Find.WorldGrid.tiles.IndexOf(tile);
                if (TileFinder.IsValidTileForNewSettlement(tileId))
                {
                    Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.Tile = tileId;
                    settlement.SetFaction(faction);

                    List<string> used = new List<string>();
                    List<Settlement> worldSettlements = Find.WorldObjects.Settlements;
                    foreach (Settlement found in worldSettlements)
                    {
                        used.Add(found.Name);
                    }

                    if (faction.def.factionNameMaker is null) Logger.Error("FactionDef " + faction.def.defName + " has no faction name maker.");
                    settlement.Name = NameGenerator.GenerateName(faction.def.factionNameMaker, used, true);
                    Find.WorldObjects.Add(settlement);
                    AddSettlement(settlement);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Add a <see cref="Settlement" /> to the <see cref="Empire" />.
        /// </summary>
        /// <param name="settlement">The <see cref="Settlement" /> to add</param>
        public void AddSettlement([NotNull] Settlement settlement)
        {
            settlements.Add(settlement, new FacilityManager(settlement));
            Territory?.SettlementClaimTiles(settlement);
        }

        /// <summary>
        ///     Add several <see cref="Settlement">Settlements</see> to the <see cref="Empire" />.
        /// </summary>
        /// <param name="settlements">The <see cref="Settlement">Settlements</see> to add</param>
        public void AddSettlements([NotNull] [ItemNotNull] IEnumerable<Settlement> settlements)
        {
            foreach (Settlement settlement in settlements)
            {
                AddSettlement(settlement);
            }
        }

        [CanBeNull]
        public Settlement GetSettlement([CanBeNull] FacilityManager manager)
        {
            if (manager == null) return null;

            foreach ((Settlement settlement, FacilityManager facilityManager) in settlements)
            {
                if (facilityManager == manager)
                {
                    return settlement;
                }
            }

            return null;
        }

        [CanBeNull]
        public FacilityManager GetFacilityManager([NotNull] Settlement settlement)
        {
            if (settlements.ContainsKey(settlement))
            {
                return settlements[settlement];
            }

            Logger.Warn($"{settlement.Name} was not in the settlement manager! Returning null.");
            return null;
        }
    }
}
