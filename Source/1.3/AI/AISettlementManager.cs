﻿using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Facilities;
using Empire_Rewritten.Settlements;
using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.AI
{
    public class AISettlementManager : AIModule
    {
        private Tile tileToBuildOn;

        public AISettlementManager([NotNull] AIPlayer player) : base(player) { }

        public bool CanUpgradeOrBuild { get; private set; }

        public bool AttemptToUpgradeSettlement([NotNull] Settlement settlement)
        {
            FacilityManager facilityManager = player.Empire?.GetFacilityManager(settlement);
            if (facilityManager == null)
            {
                Log.Warning($"{settlement.GetType().Name} has no facility manager");
                return false;
            }

            return AttemptToUpgradeSettlement(facilityManager);
        }

        public bool AttemptToUpgradeSettlement(FacilityManager manager)
        {
            if (!player.FacilityManager.CanMakeFacilities /* || otherFactor */)
            {
                //Do something
            }

            return false;
        }

        public void BuildOrUpgradeNewSettlement()
        {
            bool upgradedSettlement = false;
            if (player.Empire == null) return;
            if (player.Empire.Settlements.Count > 0)
            {
                FacilityManager facilityManager =
                    player.Empire.Settlements.Values.Where(manager => manager != null && !manager.IsFullyUpgraded).RandomElement();
                upgradedSettlement = AttemptToUpgradeSettlement(facilityManager);
            }

            // TODO: Add back this logic
            bool builtSettlement = false;
            if (!upgradedSettlement)
            {
                // AttemptBuildNewSettlement();
                SearchForTile();
            }

            CanUpgradeOrBuild = upgradedSettlement || builtSettlement;
        }

        /// <summary>
        ///     Search for tiles to build settlements on based off weights;
        ///     Weights:
        ///     - Resources
        ///     - Territory distance
        ///     Resources AI wants = higher weight
        ///     Resources AI has excess of = lower weight
        /// </summary>
        /// <returns></returns>
        public void SearchForTile()
        {
            // TODO: Pretty sure this can be improved...

            IEnumerable<int> tileOptions = player.TileManager.AllTiles;

            // Default largestWeight to -1000 so it's almost always initially overridden.
            float largestWeight = -1000;

            int result = 0;
            foreach (int tileOption in tileOptions)
            {
                float weight = player.TileManager.GetTileWeight(tileOption);
                if (largestWeight < weight && TileFinder.IsValidTileForNewSettlement(tileOption))
                {
                    largestWeight = weight;
                    result = tileOption;
                }
            }

            tileToBuildOn = Find.WorldGrid[result];
        }

        public bool AttemptToBuildSettlement()
        {
            if (tileToBuildOn != null && player.Empire != null && player.Empire.StorageTracker.CanRemoveThingsFromStorage(Empire.SettlementCost))
            {
                BuildSettlement();
                return true;
            }

            return false;
        }

        public void BuildSettlement()
        {
            if (tileToBuildOn != null && player.Empire != null)
            {
                player.Empire.BuildNewSettlementOnTile(tileToBuildOn);
                tileToBuildOn = null;
            }
        }

        public override void DoModuleAction()
        {
            SearchForTile();
            AttemptToBuildSettlement();
        }

        public override void DoThreadableAction()
        {
            // SearchForTile();
        }
    }
}
