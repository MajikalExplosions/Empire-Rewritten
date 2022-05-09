using System;
using Empire_Rewritten.Controllers;
using Empire_Rewritten.Settlements;
using JetBrains.Annotations;
using RimWorld;

namespace Empire_Rewritten.Player
{
    public class UserPlayer : BasePlayer
    {
        private bool cacheIsDirty;
        private Empire cachedEmpire;

        public UserPlayer([NotNull] Faction faction) : base(faction) { }

        [CanBeNull]
        public Empire Empire
        {
            get
            {
                if (cachedEmpire == null || cacheIsDirty)
                {
                    cacheIsDirty = false;
                    cachedEmpire = UpdateController.CurrentWorldInstance?.FactionController?.GetOwnedEmpire(faction);
                }

                return cachedEmpire;
            }
        }

        public override void MakeMove(FactionController factionController)
        {
            throw new NotImplementedException();
        }

        public override void MakeThreadedMove(FactionController factionController)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldExecute()
        {
            return false;
        }

        public override bool ShouldExecuteThreaded()
        {
            return false;
        }
    }
}
