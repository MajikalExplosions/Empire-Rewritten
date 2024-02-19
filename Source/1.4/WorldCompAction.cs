using JetBrains.Annotations;
using System;
using Verse;

namespace Empire_Rewritten
{
    /// <summary>
    /// An action that is executed by the <see cref="EmpireWorldComp"/> class on ticks where ShouldExecute returns true.
    /// </summary>
    public class WorldCompAction
    {
        /// <summary>
        /// The <see cref="Action{T}"/> to execute. Takes a single <see cref="EmpireWorldComp"/> as parameter.
        /// </summary>
        private readonly Action<EmpireWorldComp> _action;

        /// <summary>
        ///     The <see cref="Func{TResult}" /> that determines if <see cref="WorldCompAction._action" /> should be
        ///     executed
        /// </summary>
        private Func<bool> _shouldExecute { get; }

        /// <summary>
		///     The <see cref="Func{TResult}" /> that determines if <see cref="WorldCompAction._action" /> should be
		///     discarded
		/// </summary>
		private Func<bool> _shouldDiscard { get; }

        public WorldCompAction([NotNull] Action<EmpireWorldComp> action, Func<bool> shouldExecute, Func<bool> shouldDiscard)
        {
            _action = action;
            _shouldExecute = shouldExecute;
            _shouldDiscard = shouldDiscard;
        }

        public WorldCompAction([NotNull] Action<EmpireWorldComp> action, int tickInterval, int tickOffset = -1)
        {
            this._action = action;
            _shouldExecute = _ShouldExecuteDefault(tickInterval, tickOffset);
            _shouldDiscard = () => false;
        }

        private Func<bool> _ShouldExecuteDefault(int tickInterval, int tickOffset)
        {
            // TODO Use RimWorld's entity ticking hash to prevent lag spikes?
            return () => Find.TickManager.TicksGame % tickInterval == (tickOffset == -1 ? GetHashCode() : tickOffset);
        }
        public bool TryExecute(EmpireWorldComp worldComp)
        {
            if (_shouldExecute())
            {
                _action(worldComp);
                return true;
            }
            return false;
        }

        public bool ShouldDiscard()
        {
            return _shouldDiscard();
        }

        public override string ToString()
        {
            return $"[{GetType().Name}]";
        }
    }
}