using System;
using System.Linq;
using Empire_Rewritten.Controllers;
using Empire_Rewritten.Settlements;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Empire_Rewritten.Events
{
    public class EventManager : IExposable
    {
        [NotNull] private static readonly Random Rand = new Random();
        private static int _lastDay;

        public EventManager()
        {
            UpdateController.CurrentWorldInstance?.AddUpdateCall(DoRandomEventOnRandomEmpire, ShouldFireEvent);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _lastDay, "lastDay");
        }

        public static bool ShouldFireEvent()
        {
            int passedDays = GenDate.DaysPassed;

            if (passedDays - _lastDay > FactionController.DaysPerTurn)
            {
                _lastDay = passedDays;
                return true;
            }

            return false;
        }

        public static void DoRandomEventOnRandomEmpire([NotNull] FactionController controller)
        {
            Empire empire = controller.ReadOnlyFactionSettlementData.RandomElement()?.Empire;

            if (empire == null)
            {
                Logger.Warn("Faction doesn't have Empire to fire event on!");
                return;
            }

            FireRandomEvent(empire);
        }

        public static void FireRandomEvent([NotNull] Empire empire)
        {
            EventDef def = DefDatabase<EventDef>.AllDefsListForReading.Where(eventDef => !empire.IsAIPlayer || eventDef.canAffectAI).RandomElement();
            if (def == null)
            {
                Logger.Warn("No events to fire found!");
                return;
            }

            EventWorker worker = empire.IsAIPlayer ? def.AIWorker : def.EventWorker;
            if (worker?.Chance > Rand.Next(0, 100))
            {
                worker.Event(empire);
            }
        }
    }
}
