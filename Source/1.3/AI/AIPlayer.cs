using System.Threading.Tasks;
using Empire_Rewritten.Controllers;
using Empire_Rewritten.Settlements;
using JetBrains.Annotations;
using RimWorld;

namespace Empire_Rewritten.AI
{
    public class AIPlayer : BasePlayer
    {
        private bool cacheIsDirty = true;
        [CanBeNull] private Empire cachedEmpire;

        private int threadTick;

        private int tick;

        public AIPlayer([NotNull] Faction faction) : base(faction)
        {
            ResourceManager = new AIResourceManager(this);
            SettlementManager = new AISettlementManager(this);
            FacilityManager = new AIFacilityManager(this);
            TileManager = new AITileManager(this);
        }

        [CanBeNull]
        public Empire Empire
        {
            get
            {
                if (cachedEmpire == null || cacheIsDirty)
                {
                    cacheIsDirty = false;

                    cachedEmpire = UpdateController.CurrentWorldInstance?.FactionController?.GetOwnedEmpire(Faction);
                }

                return cachedEmpire;
            }
        }

        [NotNull] public AITileManager TileManager { get; }
        [NotNull] public AISettlementManager SettlementManager { get; }
        [NotNull] public AIFacilityManager FacilityManager { get; }
        [NotNull] public AIResourceManager ResourceManager { get; }
        [NotNull] public Faction Faction => faction;

        public override void MakeMove(FactionController factionController)
        {
            ResourceManager.DoModuleAction();
            FacilityManager.DoModuleAction();
            SettlementManager.DoModuleAction();
            TileManager.DoModuleAction();
        }

        public override bool ShouldExecute()
        {
            if (tick == 120)
            {
                tick = 0;
                return true;
            }

            tick++;
            return false;
        }

        public override void MakeThreadedMove(FactionController factionController)
        {
            Task.Run(SettlementManager.DoThreadableAction);
        }

        public override bool ShouldExecuteThreaded()
        {
            if (threadTick == 2)
            {
                threadTick = 0;
                return true;
            }

            threadTick++;
            return false;
        }
    }
}
