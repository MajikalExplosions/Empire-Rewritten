using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Empire_Rewritten.Facilities.FacilityWorkers
{
    public class TestWorker : FacilityWorker
    {
        public TestWorker(FacilityDef facilityDef) : base(facilityDef) { }

        [NotNull]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos() ?? Enumerable.Empty<Gizmo>();
        }
    }
}
