using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.Events
{
    public class EventManager : IExposable
    {
        private static List<EventDef> _eventCache = new List<EventDef>();

        public EventManager()
        {
            _eventCache = DefDatabase<EventDef>.AllDefsListForReading;
        }

        // Ticks hourly.
        public void Tick(EmpireWorldComp e) { }

        public void ExposeData()
        {
            // TODO Does eventmanager even have a state?
        }
    }
}
