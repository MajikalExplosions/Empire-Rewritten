using Empire_Rewritten.AI;
using Empire_Rewritten.Resources;
using RimWorld;
using System.Collections.Generic;

namespace Empire_Rewritten.Player
{
    public class AIPlayer : BasePlayer
    {

        private AIResourceManager _resourceManager;
        private AIExpansionManager _expansionManager;

        public AIPlayer(Faction faction) : base(faction)
        {
            _resourceManager = new AIResourceManager(this);
            _expansionManager = new AIExpansionManager(this);
        }


        public override void MakeMove(EmpireWorldComp worldComp)
        {
            _resourceManager.DoModuleAction();
            _expansionManager.DoModuleAction();
        }

        public override bool IsAI()
        {
            return true;
        }

        public Dictionary<ResourceDef, float> GetResourcePriority()
        {
            return _resourceManager.ResourcePriority;
        }
    }
}