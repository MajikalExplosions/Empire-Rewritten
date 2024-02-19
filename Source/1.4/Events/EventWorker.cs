using RimWorld;
namespace Empire_Rewritten.Events
{
    public class EventWorker
    {
        public EventDef def;
        public virtual float Chance
        {
            get
            {
                return 0f;
            }
        }
        public virtual void Event(Faction faction)
        {

        }
    }
}
