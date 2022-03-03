﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire_Rewritten.AI
{
    public class AIPlayer : BasePlayer
    {
        private AIFacilityManager facilityManager;
        private AISettlementManager settlementManager;
        private AIResourceManager resourceManager;
        private SettlementManager cachedManager;
        private bool ManagerIsDirty = true;

        public SettlementManager Manager
        {
            get
            {
                if (cachedManager== null || ManagerIsDirty)
                {
                    ManagerIsDirty = true;
                    UpdateController updateController = UpdateController.GetUpdateController;
                    FactionController factionController = updateController.FactionController;

                    cachedManager = factionController.GetOwnedSettlementManager(Faction);
                }
                return cachedManager;
            }
        }

        public AIFacilityManager FacilityManager
        {
            get
            {
                return facilityManager;
            }
        }

        public Faction Faction
        {
            get
            {
                return faction;
            }
        }

        public AIPlayer(Faction faction) : base(faction)
        {
            this.resourceManager = new AIResourceManager(this);
            this.settlementManager = new AISettlementManager(this);
            this.facilityManager = new AIFacilityManager(this);
        }

        public AIResourceManager ResourceManager
        {
            get
            {
                return resourceManager;
            }
        }

        public override void MakeMove()
        {
            throw new NotImplementedException();
        }
    }
}