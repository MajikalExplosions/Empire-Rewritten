using Empire_Rewritten.Resources;
using JetBrains.Annotations;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.Settlements
{
    public class Facility : IExposable, ILoadReferenceable
    {
        public FacilityDef def;

        private int _size;
        public int Size
        {
            get => _size;
            private set => _size = value;
        }

        private Settlement _settlement;
        public Settlement Settlement
        {
            get => _settlement;
            private set => _settlement = value;
        }

        public Facility(FacilityDef def, Settlement settlement)
        {
            this.def = def;
            _settlement = settlement;
            _size = 0;
        }

        [UsedImplicitly]
        public Facility() { }

        /// <summary>
        /// Computes the production of this <see cref="Facility"/> by using the base productions (offsets) of the <see cref="FacilityDef"/>
        /// and the total multipliers of the <see cref="Settlement"/>.
        /// </summary>
        /// <returns></returns>
        public List<ResourceModifier> GetProduction()
        {
            List<ResourceModifier> producedResources = new List<ResourceModifier>();
            foreach (ResourceModifier resourceModifier in def.production)
            {
                float multiplier = _settlement.GetDetails().GetProductionMultiplierFor(resourceModifier.def);

                // We use the whole settlement's multiplier, which includes the multiplier from this facility.
                ResourceModifier modifier = new ResourceModifier(resourceModifier.def, resourceModifier.offset * _size, multiplier);
                producedResources.Add(modifier);
            }

            return producedResources;
        }

        public List<ResourceModifier> GetUpkeep()
        {
            List<ResourceModifier> upkeep = new List<ResourceModifier>();
            foreach (ResourceModifier resourceModifier in def.upkeep)
            {
                float multiplier = _settlement.GetDetails().GetUpkeepMultiplierFor(resourceModifier.def);

                // We use the whole settlement's multiplier, which includes the multiplier from this facility.
                ResourceModifier modifier = new ResourceModifier(resourceModifier.def, resourceModifier.offset * _size, multiplier);
                upkeep.Add(modifier);
            }

            return upkeep;
        }

        /// <summary>
        ///     Build some size of this <see cref="Facility" /> to its settlement.
        /// </summary>
        /// <param name="size">The additional <see cref="int">size</see> of facility to add</param>
        public void ChangeSize(int size)
        {
            _size += size;
            if (_size < 0) _size = 0;

            if (_size > Settlement.GetDetails().GetMaxFacilitySize())
                _size = Settlement.GetDetails().GetMaxFacilitySize();

            if (size > 0)
            {
                //this.def.facilityWorker?.NotifyConstructed(this);
            }
            else if (size < 0)
            {
                //this.def.facilityWorker?.NotifyDemolished(this);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref _size, "size");
            Scribe_References.Look(ref _settlement, "settlement");
        }

        public string GetUniqueLoadID()
        {
            return $"Facility_{GetHashCode()}";
        }
    }
}