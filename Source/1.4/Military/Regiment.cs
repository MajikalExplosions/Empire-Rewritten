using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Empire_Rewritten.Military
{
    public class Regiment : IExposable
    {
        private int _strength;

        public int Strength
        {
            get
            {
                if (_units.Count == 0)
                {
                    return _strength;
                }
                else
                {
                    return _units.Sum(u => u.Key.UnitStrength() * u.Value);
                }
            }
            set
            {
                if (value < 0)
                {
                    _strength = 0;
                }
                else
                {
                    _strength = value;
                    _units = new Dictionary<UnitTemplate, int>();
                }
            }
        }

        private Dictionary<UnitTemplate, int> _units;
        public Dictionary<UnitTemplate, int> Units
        {
            get => _units;
        }

        public Regiment()
        {
            _units = new Dictionary<UnitTemplate, int>();
            _strength = 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _strength, "strength");
            Scribe_Collections.Look(ref _units, "units", LookMode.Reference, LookMode.Value);
        }
    }
}
