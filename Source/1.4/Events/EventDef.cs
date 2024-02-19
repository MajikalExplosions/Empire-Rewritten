using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.Events
{

    [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class EventDef : Def
    {
        // TODO Complete this class. I have no clue what's going on here.

        private EventWorker aiWorker;
        public Type eventWorker;

        public EventWorker EventWorker { get; private set; }

        public EventWorker AIWorker => aiWorker ?? EventWorker;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            /*
            if (eventWorker != typeof(EventWorker) && !eventWorker.IsSubclassOf(typeof(EventWorker)))
            {
                yield return $"{eventWorker.Name} is not a {nameof(Events.EventWorker)}";
            }

            if (aiEventWorker != null && aiEventWorker != typeof(EventWorker) &&
                !aiEventWorker.IsSubclassOf(typeof(EventWorker)))
            {
                yield return $"{aiEventWorker.Name} is not a {nameof(Events.EventWorker)}";
            }
            */
        }

        public override void ResolveReferences()
        {
            /*
            if (eventWorker != typeof(EventWorker) && !eventWorker.IsSubclassOf(typeof(EventWorker)))
            {
                EventWorker = (EventWorker)Activator.CreateInstance(eventWorker);
                EventWorker.def = this;
            }

            if (aiEventWorker != null && aiEventWorker != typeof(EventWorker) &&
                !aiEventWorker.IsSubclassOf(typeof(EventWorker)))
            {
                aiWorker = (EventWorker)Activator.CreateInstance(aiEventWorker);
                aiWorker.def = this;
            }

            base.ResolveReferences();
            */
        }
    }
}
