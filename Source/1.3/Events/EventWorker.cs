using Empire_Rewritten.Settlements;

namespace Empire_Rewritten.Events
{
    public class EventWorker
    {
        public EventDef def;

        public virtual float Chance => 0f;

        public virtual void Event(Empire empire) { }
    }
}
