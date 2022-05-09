using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Controllers;
using Empire_Rewritten.Facilities;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Settlements
{
    public static class SettlementExtensions
    {
        /// <summary>
        ///     Gets the <see cref="Empire" /> of a given <see cref="Settlement" />.
        /// </summary>
        /// <param name="settlement">The <see cref="Settlement" /> to get the <see cref="Empire" /> of</param>
        /// <returns>The <see cref="Empire" /> of <paramref name="settlement" /></returns>
        [CanBeNull]
        public static Empire GetManager([NotNull] this Settlement settlement)
        {
            return UpdateController.CurrentWorldInstance?.FactionController?.GetOwnedEmpire(settlement.Faction);
        }

        /// <summary>
        ///     Gets all <see cref="Gizmo">Gizmos</see> provided by <see cref="Facility">Facilities</see> of a given
        ///     <see cref="Settlement" />.
        /// </summary>
        /// <param name="settlement">The <see cref="Settlement" /> to get the <see cref="Gizmo">Gizmos</see> of</param>
        /// <returns>The <see cref="Gizmo">Gizmos</see> of <paramref name="settlement" /></returns>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Gizmo> GetExtendedGizmos([NotNull] this Settlement settlement)
        {
            if (settlement.Faction != Faction.OfPlayer) return Enumerable.Empty<Gizmo>();

            return GetManager(settlement)?.GetFacilityManager(settlement)?.GetGizmos() ?? Enumerable.Empty<Gizmo>();
        }
    }
}
