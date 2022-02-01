﻿using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Empire_Rewritten.AI
{
    public class AISettlementManager : AIModule
    {
        public AISettlementManager(AIPlayer player) : base(player)
        {
        }

        private bool canUpgradeOrBuild;

        public bool CanUpgradeOrBuild
        {
            get
            {
                return canUpgradeOrBuild;
            }
        }
        
  
        public bool AttemptToUpgradeSettlement(Settlement settlement)
        {
            FacilityManager facilityManager = player.Manager.GetFacilityManager(settlement);

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


        public bool AttemptBuildNewSettlement()
        {
            if (!player.FacilityManager.CanMakeFacilities /* || otherFactor */)
            {
                //Do something
            }

            return false;
        }

        public void BuildOrUpgradeNewSettlement()
        {
            KeyValuePair<Settlement,FacilityManager> settlementAndManager = player.Manager.Settlements.Where(x => true /* !x.Value.IsFullyUpgraded*/).RandomElement();
            Settlement settlement = settlementAndManager.Key;
            FacilityManager facilityManager = settlementAndManager.Value;
            bool UpgradedSettlement = AttemptToUpgradeSettlement(facilityManager);
            bool BuiltSettlement = false;
            if (!UpgradedSettlement)
            {
                BuiltSettlement = AttemptBuildNewSettlement();
            }
            
            canUpgradeOrBuild = UpgradedSettlement || BuiltSettlement;
        }


        /// <summary>
        /// Search for tiles to build settlements on based off weights;
        /// Weights:
        /// - Resources
        /// - Border distance
        /// 
        /// Resources AI wants = higher weight
        /// Resources AI has excess of = lower weight
        /// </summary>
        /// <returns></returns>
        public Tile SearchForTile()
        {
            Tile t = null;

            //temp test
            //todo when bordermanager is implimented:
            //only pull from owned tiles.
            List<Tile> tiles = Find.WorldGrid.tiles;
            AIResourceManager aIResourceManager = player.ResourceManager;

            Dictionary<float, List<Tile>> tileWeights = new Dictionary<float, List<Tile>>();

            foreach (Tile tile in tiles)
            {
                float weight = aIResourceManager.GetTileResourceWeight(tile);

                if (tileWeights.ContainsKey(weight))
                {
                    tileWeights[weight].Add(tile);
                }
                else
                {
                    tileWeights.Add(weight, new List<Tile>() { tile });
                }

                /*
                todo: border weight
                */
            }
            //This should be smarter in the future.
            float largestWeight = tileWeights.Keys.Max();
            t = tileWeights[largestWeight].RandomElement();

            return t;
        }

        public override void DoModuleAction()
        {
            
        }
    }
}
