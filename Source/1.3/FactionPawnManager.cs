using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Empire_Rewritten
{
    public struct FactionPawns : IExposable
    {
        [NotNull] public Faction faction;
        [NotNull] [ItemNotNull] public List<Pawn> pawns;

        public void ExposeData()
        {
            Scribe_References.Look(ref faction, nameof(faction));
            Scribe_Collections.Look(ref pawns, nameof(pawns), LookMode.Reference);
        }

        public bool IsForFaction([CanBeNull] Faction otherFaction)
        {
            return otherFaction != null && otherFaction == faction;
        }

        public bool HasPawn([CanBeNull] Pawn pawn)
        {
            return pawns.Contains(pawn);
        }
    }

    public class FactionPawnManager : IExposable
    {
        [NotNull] private List<FactionPawns> factionPawns = new List<FactionPawns>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref factionPawns, "factionPawns", LookMode.Deep);
        }

        /// <summary>
        ///     Check if we have the faction stored.
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        public bool HasFaction(Faction faction)
        {
            return factionPawns.Any(fp => fp.IsForFaction(faction));
        }

        /// <summary>
        ///     Get all pawns for a faction.
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        public List<Pawn> PawnsForFaction(Faction faction)
        {
            List<Pawn> result = new List<Pawn>();

            if (HasFaction(faction))
            {
                return result;
            }

            result = factionPawns.Find(fp => fp.faction == faction).pawns;

            return result;
        }

        /// <summary>
        ///     Generate a pawn for a faction
        /// </summary>
        /// <param name="faction">Target faction</param>
        public void GeneratePawnForFaction([NotNull] Faction faction)
        {
            PawnGenerationRequest generationRequest =
                new PawnGenerationRequest(faction.RandomPawnKind(), faction, mustBeCapableOfViolence: true, fixedIdeo: faction.ideos?.PrimaryIdeo);
            Pawn result = PawnGenerator.GeneratePawn(generationRequest);

            FactionPawns facPawns = HasFaction(faction)
                ? factionPawns.Find(pawn => pawn.faction == faction)
                : new FactionPawns { faction = faction, pawns = new List<Pawn>() };

            facPawns.pawns.Add(result);

            //Replace at index
            int index = factionPawns.IndexOf(facPawns);
            factionPawns.RemoveAt(index);
            factionPawns.Insert(index, facPawns);
        }

        /// <summary>
        ///     Generate multiple pawns for a faction
        /// </summary>
        /// <param name="faction">Target faction</param>
        /// <param name="amount">Amount of pawns to generate</param>
        public void GeneratePawnsForFaction([NotNull] Faction faction, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GeneratePawnForFaction(faction);
            }
        }
    }
}
