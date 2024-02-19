using Empire_Rewritten.Settlements;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.Military
{

    public class MilitaryManager : IExposable
    {
        // Note that templates/pawns are player-exclusive
        private List<UnitTemplate> _templates;
        public List<UnitTemplate> Templates => _templates;

        private List<Pawn> _pawns;
        public List<Pawn> AllPawns => _pawns;

        public MilitaryManager()
        {
            _templates = new List<UnitTemplate>();
            _pawns = new List<Pawn>();
        }

        public bool RemovePawn(Pawn pawn)
        {
            if (_pawns.Contains(pawn))
            {
                _pawns.Remove(pawn);
                return true;
            }
            return false;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _templates, "templates", LookMode.Deep);
            Scribe_Collections.Look(ref _pawns, "pawns", LookMode.Reference);
        }
    }
}