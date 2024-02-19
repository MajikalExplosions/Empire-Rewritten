using RimWorld;

namespace Empire_Rewritten.Player
{
    /// <summary>
    ///     The base player. Triggers player action/logic.
    /// </summary>
    public abstract class BasePlayer
    {
        public Faction Faction { get; private set; }
        public BasePlayer(Faction faction)
        {
            Faction = faction;
        }

        /// <summary>
        ///     Make moves for this faction
        /// </summary>
        public abstract void MakeMove(EmpireWorldComp worldComp);

        /// <summary>
        /// Whether this is an AI
        /// </summary>
        /// <returns>True iff this faction is played by AI</returns>
        public abstract bool IsAI();
    }
}