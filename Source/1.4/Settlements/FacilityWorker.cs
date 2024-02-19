using Empire_Rewritten.Player;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Empire_Rewritten.Settlements
{
    /// <summary>
    ///     Used to define actions for facility types.
    /// </summary>
    public abstract class FacilityWorker
    {
        public readonly FacilityDef facilityDef;

        public FacilityWorker(FacilityDef facilityDef)
        {
            this.facilityDef = facilityDef;
        }

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            return Enumerable.Empty<Gizmo>();
        }

        public virtual bool CanBuildAt(Settlement settlement)
        {
            return true;
        }

        public virtual bool CanBeBuiltBy(BasePlayer basePlayer)
        {
            return true;
        }
    }
}