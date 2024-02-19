using Empire_Rewritten.Settlements;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Processes
{
    /// <summary>
    ///     A type of <see cref="Process"/> used to construct a new <see cref="Facility"/> inside a <see cref="Facility
    ///     "/>
    /// </summary>
    internal class FacilityBuildProcess : Process
    {
        private Settlement _settlement;
        private FacilityDef _facilityDef;
        public FacilityDef FacilityDef => _facilityDef;

        /// <summary>
        ///     Used for saving/loading
        /// </summary>
        public FacilityBuildProcess() : base() { }

        /// <summary>
        ///     Creates a new <see cref="FacilityBuildProcess"/>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toolTip"></param>
        /// <param name="duration"></param>
        /// <param name="iconPath"></param>
        /// <param name="facilityManager"></param>
        /// <param name="facilityDef"></param>
        /// <param name="slotID"></param>
        public FacilityBuildProcess(string label, string toolTip, int duration, string iconPath, Settlement settlement, FacilityDef facilityDef) : base(label, toolTip, duration, iconPath)
        {
            _settlement = settlement;
            _facilityDef = facilityDef;
        }

        public FacilityBuildProcess(Settlement settlement, FacilityDef facilityDef) : base("", "", 1, "")// TODO Change iconPath to some default building sprite
        {
            _settlement = settlement;
            _facilityDef = facilityDef;
            Label = "Building " + facilityDef.label;
            Tooltip = "Building " + facilityDef.label;
            Duration = facilityDef.buildDuration;
        }

        /// <summary>
        ///     Overrides <see cref="Process.Run"/>; Builds a new <see cref="Facility"/>, or adds to it using <see cref="Facility.AddFacility"/> if it already exists, inside the given <see cref="FacilityManager"/> <paramref name="facilityManager"/> using the <see cref="FacilityDef"/> <paramref name="facilityDef"/>; Notifies the <see cref="facilityManager"/>
        /// </summary>
        protected override void OnComplete()
        {
            _settlement.GetDetails().ChangeFacilitySize(_facilityDef, 1);

            // Remove the process from the settlement
            _settlement.GetDetails().RemoveProcess(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _settlement, "settlement");
            Scribe_Defs.Look(ref _facilityDef, "facilityDef");
        }

        public override void Cancel()
        {
            base.Cancel();
            // Remove the process from the settlement
            _settlement.GetDetails().RemoveProcess(this);
        }
    }
}
