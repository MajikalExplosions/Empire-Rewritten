using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Settlements;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Empire_Rewritten.Controllers
{
    /// <summary>
    ///     Links a <see cref="Faction" /> and its <see cref="RimWorld.Planet.Settlement" /> through a
    ///     <see cref="Empire" />
    /// </summary>
    public class FactionSettlementData : IExposable
    {
        [NotNull] public Faction owner;
        [NotNull] private Empire empire;
        [NotNull] private Faction originalOwner;

        /// <summary>
        ///     Used for saving/loading
        /// </summary>
        [UsedImplicitly]
        public FactionSettlementData() { }

        /// <summary>
        ///     Supposed to be called when a <see cref="Faction" /> is created
        /// </summary>
        /// <param name="owner">The <see cref="Faction" /> that this <see cref="FactionSettlementData" /> belongs to</param>
        /// <param name="empire">
        ///     The <see cref="Empire" /> of this <see cref="FactionSettlementData" />
        /// </param>
        public FactionSettlementData([NotNull] Faction owner, [NotNull] Empire empire)
        {
            this.owner = owner;
            originalOwner = owner;
            this.empire = empire;
        }

        /// <summary>
        ///     The <see cref="Faction" /> that originally created this <see cref="FactionSettlementData" />
        ///     Should never change
        /// </summary>
        [NotNull]
        public Faction OriginalOwner => originalOwner;

        /// <summary>
        ///     Returns the Empire<see cref="Settlements.Empire" />, shouldn't ever be changed
        /// </summary>
        [NotNull]
        public Empire Empire => empire;

        public void ExposeData()
        {
            Scribe_References.Look(ref owner, "owner");
            Scribe_References.Look(ref originalOwner, "originalOwner");
            Scribe_Deep.Look(ref empire, "empireempire");
        }

        /// <summary>
        ///     Creates all required instances of <see cref="FactionSettlementData" />
        /// </summary>
        /// <returns>A <see cref="List{T}" /> of the newly created <see cref="FactionSettlementData" /> instances</returns>
        [NotNull]
        internal static List<FactionSettlementData> CreateFactionSettlementData()
        {
            return Find.FactionManager.AllFactionsListForReading.Select(faction => new FactionSettlementData(faction, new Empire(faction))).ToList();
        }

        public override string ToString()
        {
            return $"[FactionSettlementData] owner: {owner.Name ?? "<null>"}, originalOwner: {originalOwner.Name ?? "<null>"}, empire: {empire}";
        }
    }
}
