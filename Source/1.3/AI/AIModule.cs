using JetBrains.Annotations;

namespace Empire_Rewritten.AI
{
    public abstract class AIModule
    {
        [NotNull] public AIPlayer player;

        public AIModule([NotNull] AIPlayer player)
        {
            this.player = player;
        }

        public abstract void DoModuleAction();

        /// <summary>
        ///     These actions can be loaded to another thread.
        /// </summary>
        public abstract void DoThreadableAction();
    }
}
