using Empire_Rewritten.Player;
using Empire_Rewritten.Resources;
using System.Collections.Generic;

namespace Empire_Rewritten.AI
{
    public class AIResourceManager : AIModule
    {
        // How much the AI wants to build each resource.
        public Dictionary<ResourceDef, float> ResourcePriority = new Dictionary<ResourceDef, float>();
        public AIResourceManager(AIPlayer player) : base(player) { }

        private void _ComputeResourcePrio()
        {
            Dictionary<ResourceDef, float> stockpile = _player.Faction.GetStockpile();
            Dictionary<ResourceDef, float> change = _player.Faction.GetResourceChange();

            ResourcePriority.Clear();

            // We assume that if we're producing it, it's in the stockpile.
            foreach (ResourceDef resource in stockpile.Keys)
            {
                float prio = 0;
                float resChange = change.ContainsKey(resource) ? change[resource] : 0;
                // The fewer days we have in stockpile, the more we want to build.
                float daysRemaining = resChange < 0 ? stockpile[resource] / -resChange :
                    1000 + resChange * 10;// If we have a surplus, scale up daysRemaining based on surplus.

                prio += 1 / daysRemaining;
                ResourcePriority.Add(resource, prio);
            }
        }

        public override void DoModuleAction()
        {
            _ComputeResourcePrio();
        }
    }
}
