using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Empire_Rewritten.Events
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class EventDef : Def
    {
        public bool canAffectAI = true;

        [CanBeNull] public Type aiEventWorker;
        [NotNull] public Type eventWorker;

        private EventWorker aiWorker;

        public EventWorker EventWorker { get; private set; }
        public EventWorker AIWorker => aiWorker ?? EventWorker;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors() ?? Enumerable.Empty<string>())
            {
                yield return error;
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            if (eventWorker == null)
            {
                yield return $"no {nameof(eventWorker)} given";
            }
            else if (!eventWorker.IsSubclassOf(typeof(EventWorker)))
            {
                yield return $"{eventWorker.Name} is not a {nameof(Events.EventWorker)}";
            }

            if (aiEventWorker != null && aiEventWorker != typeof(EventWorker) && !aiEventWorker.IsSubclassOf(typeof(EventWorker)))
            {
                yield return $"{aiEventWorker.Name} is not a {nameof(Events.EventWorker)}";
            }
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        public override void ResolveReferences()
        {
            EventWorker = (EventWorker)Activator.CreateInstance(eventWorker);
            if (EventWorker == null)
            {
                throw new NullReferenceException($"{nameof(EventWorker)} {eventWorker.Name} is null");
            }

            EventWorker.def = this;

            if (aiEventWorker != null)
            {
                aiWorker = (EventWorker)Activator.CreateInstance(aiEventWorker);
                if (aiWorker == null)
                {
                    throw new NullReferenceException($"{nameof(EventWorker)} {aiEventWorker.Name} is null");
                }

                aiWorker.def = this;
            }

            base.ResolveReferences();
        }
    }
}
