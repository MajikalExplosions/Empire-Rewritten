using Empire_Rewritten.Military;
using Empire_Rewritten.Resources;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Empire_Rewritten.Factions
{
    public class FactionDetails : IExposable
    {
        private Faction _faction;
        public Faction Faction
        {
            get => _faction;
            set => _faction = value;
        }

        private Dictionary<ResourceDef, float> _stockpile;
        public Dictionary<ResourceDef, float> Stockpile
        {
            get => _stockpile;
            set => _stockpile = value;
        }

        private Emblem _emblem;
        public Emblem Emblem
        {
            get => _emblem;
            set => _emblem = value;
        }

        private List<Regiment> _regiments;
        public List<Regiment> Regiments => _regiments;

        public FactionDetails(Faction f, Emblem e = null)
        {
            _faction = f;
            _stockpile = new Dictionary<ResourceDef, float>();

            if (e == null) e = Emblem.GetRandomEmblem();
            _emblem = e;
        }

        public int RegimentStrength()
        {
            return _regiments.Sum(r => r.Strength);
        }

        public int RegimentUnitCount()
        {
            return _regiments.Sum(r => r.Units.Count);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _stockpile, "stockpile", LookMode.Def, LookMode.Value);
            Scribe_Deep.Look(ref _emblem, "emblem");
            Scribe_Collections.Look(ref _regiments, "regiments", LookMode.Deep);
        }
    }
}
