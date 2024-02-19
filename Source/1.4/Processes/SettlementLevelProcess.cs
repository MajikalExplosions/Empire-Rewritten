using Empire_Rewritten.Settlements;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Processes
{
    /// <summary>
    ///     A type of <see cref="Process"/> used to construct a new <see cref="Facility"/> inside a <see cref="FacilityManager"/>
    /// </summary>
    internal class SettlementLevelProcess : Process
    {
        private const int _UpgradeDuration = 3 * EmpireWorldComp.TICK_DAILY;
        private Settlement _settlement;

        /// <summary>
        ///     Used for saving/loading
        /// </summary>
        public SettlementLevelProcess() : base() { }

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
        public SettlementLevelProcess(string label, string toolTip, int duration, string iconPath, Settlement settlement) : base(label, toolTip, duration, iconPath)
        {
            _settlement = settlement;
        }

        public SettlementLevelProcess(Settlement settlement) : base("", "", 1, "")// TODO Change iconPath to some default building sprite
        {
            _settlement = settlement;
            Label = "Upgrading Settlement";
            Tooltip = "Upgrading Settlement " + settlement.Name + " to level " + (settlement.GetDetails().Level + 1).ToString();
            Duration = _UpgradeDuration;
        }

        /// <summary>
        ///     Overrides <see cref="Process.Run"/>; Builds a new <see cref="Facility"/>, or adds to it using <see cref="Facility.AddFacility"/> if it already exists, inside the given <see cref="FacilityManager"/> <paramref name="facilityManager"/> using the <see cref="FacilityDef"/> <paramref name="facilityDef"/>; Notifies the <see cref="facilityManager"/>
        /// </summary>
        protected override void OnComplete()
        {
            _settlement.GetDetails().Level++;

            // Remove the process from the settlement
            _settlement.GetDetails().RemoveProcess(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref _settlement, nameof(_settlement));
        }

        public override void Cancel()
        {
            base.Cancel();
            // Remove the process from the settlement
            _settlement.GetDetails().RemoveProcess(this);
        }
    }
}
