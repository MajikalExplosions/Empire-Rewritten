using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.Processes
{
    public class PlayerMapDeliveryProcess : Process, IExposable
    {
        private Map map;
        private List<Thing> things;

        public PlayerMapDeliveryProcess(int duration, Map map, List<Thing> things) : base("Delivery to Player Base", "Items are being delivered", duration, "")// TODO Find icon path
        {
            this.map = map;
            this.things = things;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref map, nameof(map));
            Scribe_Collections.Look(ref things, nameof(things), LookMode.Deep);
        }

        protected override void OnComplete()
        {
            map = map ?? Find.AnyPlayerHomeMap;
            IntVec3 position = _TryGetTaxSpotLocation(map) ?? new IntVec3();

            foreach (Thing thing in things)
            {
                GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near);
            }
        }

        private static IntVec3? _TryGetTaxSpotLocation(Map map) => map.listerThings.ThingsOfDef(EmpireDefOf.EmpireTaxSpot).FirstOrFallback()?.Position;
    }
}
