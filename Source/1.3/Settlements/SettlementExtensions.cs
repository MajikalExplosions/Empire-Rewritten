﻿using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Controllers;
using Empire_Rewritten.Facilities;
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
        public static Empire GetManager(this Settlement settlement)
        {
            return UpdateController.CurrentWorldInstance.FactionController.GetOwnedSettlementManager(settlement.Faction);
        }

        /// <summary>
        ///     Gets all <see cref="Gizmo">Gizmos</see> provided by <see cref="Facility">Facilities</see> of a given
        ///     <see cref="Settlement" />.
        /// </summary>
        /// <param name="settlement">The <see cref="Settlement" /> to get the <see cref="Gizmo">Gizmos</see> of</param>
        /// <returns>The <see cref="Gizmo">Gizmos</see> of <paramref name="settlement" /></returns>
        public static IEnumerable<Gizmo> GetExtendedGizmos(this Settlement settlement)
        {
            return settlement.Faction == Faction.OfPlayer ? GetManager(settlement).GetFacilityManager(settlement).GetGizmos() : Enumerable.Empty<Gizmo>();
        }
    }
}