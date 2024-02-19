using Empire_Rewritten.Settlements;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Empire_Rewritten.Territories
{
    /// <summary>
    ///     Manages faction <see cref="Territory">territories</see>. There is one per <see cref="RimWorld.Planet.World" />.
    /// </summary>
    public class TerritoryManager : IExposable
    {
        private Dictionary<int, TileClaimants> _claims = new Dictionary<int, TileClaimants>();
        private Dictionary<Settlement, List<int>> _territoryCache = new Dictionary<Settlement, List<int>>();

        public TerritoryManager() { }

        public void AddClaimsIfMissing(Settlement settlement)
        {
            Settlement owner = GetTileOwner(settlement.Tile);
            if (owner == null || owner != settlement) AddClaims(settlement);
        }

        public void AddClaims(Settlement settlement, bool reset = false)
        {
            int radius = settlement.GetDetails().GetTerritoryRadius();
            // Clear the cache for this settlement.
            _territoryCache[settlement] = new List<int>();

            // First, create a dict of all the tiles we're claiming and their weights.
            Dictionary<int, float> tiles = new Dictionary<int, float>();

            // Use BFS to find nearby tiles; use a second queue to keep track of the distance from the settlement.
            Queue<int> queue = new Queue<int>();
            Queue<int> nextQueue = new Queue<int>();
            HashSet<int> enqueued = new HashSet<int>();
            queue.Enqueue(settlement.Tile);
            enqueued.Add(settlement.Tile);

            int distance = 0;
            string __tmp;
            while (queue.Count > 0 && distance <= radius)
            {
                int tile = queue.Dequeue();

                // Add a tiny random value to the weight to prevent ties.
                tiles.Add(tile, 1f / (distance + 1) + Rand.Value * 0.00001f);

                List<int> neighbors = new List<int>();
                Find.WorldGrid.GetTileNeighbors(tile, neighbors);
                foreach (int neighbor in neighbors)
                {
                    if (!enqueued.Contains(neighbor) && SettlementDetails.CanBuildAt(neighbor, out __tmp))
                    {
                        nextQueue.Enqueue(neighbor);
                        enqueued.Add(neighbor);
                    }
                }
                if (queue.Count == 0)
                {
                    distance++;
                    queue = nextQueue;
                    nextQueue = new Queue<int>();
                }
            }

            // Now, add the claims to the claims dict.
            foreach (KeyValuePair<int, float> pair in tiles)
            {
                if (reset && _claims[pair.Key].Claims.ContainsKey(settlement)) continue;

                // Add tile to claims dict if it doesn't exist.
                if (!_claims.ContainsKey(pair.Key)) _claims.Add(pair.Key, new TileClaimants());
                else
                {
                    // Clear the caches of all settlements that have a claim on this tile if it already did
                    //   because the weight will change.

                    // TODO Optimization: Only clear the cache of the settlement that had the claim?
                    //   I think that should work, but in case it doesn't, I'm leaving it like this.
                    foreach (KeyValuePair<Settlement, float> claim in _claims[pair.Key].Claims)
                    {
                        _territoryCache[claim.Key].Clear();
                    }

                }

                // Save the current owner
                Settlement owner = GetTileOwner(pair.Key);

                // Add settlement to claims dict, removing the existing value if it exists.
                if (_claims[pair.Key].Claims.ContainsKey(settlement)) _claims[pair.Key].Claims.Remove(settlement);
                _claims[pair.Key].Claims.Add(settlement, pair.Value);

                // If the new owner is different, tell TerritoryDrawer to redraw the tile.
                if (owner != GetTileOwner(pair.Key)) TerritoryDrawer.dirty = true;
            }
        }

        /// <summary>
        ///     Gets the <see cref="Faction" /> that owns a given world tile.
        /// </summary>
        /// <param name="tileId">The <see cref="int">ID</see> of the world tile to check</param>
        /// <returns>The <see cref="Faction" /> that owns <paramref name="tileId" />; <c>null</c> if the tile not owned</returns>
        [CanBeNull]
        public Settlement GetTileOwner(int tileId)
        {
            if (_claims.TryGetValue(tileId, out TileClaimants dict))
            {
                // Find the settlement with the highest claim value and return that.

                KeyValuePair<Settlement, float> highest = new KeyValuePair<Settlement, float>(null, 0);
                foreach (KeyValuePair<Settlement, float> pair in dict.Claims)
                {
                    if (pair.Value > highest.Value)
                    {
                        highest = pair;
                    }
                }
                return highest.Key;
            }
            return null;
        }

        public List<int> GetTerritory(Settlement settlement)
        {
            // If the settlement's territory is cached and is not empty, return it.
            if (_territoryCache.TryGetValue(settlement, out List<int> territory) && territory.Count > 0) return territory;

            // Get all owned tiles (GetTileOwner)
            territory = new List<int>();
            foreach (KeyValuePair<int, TileClaimants> pair in _claims)
            {
                if (GetTileOwner(pair.Key) == settlement) territory.Add(pair.Key);
            }
            // Warn if the settlement has no territory.
            if (territory.Count == 0) Logger.Warn($"Settlement {settlement.Name} has no territory!");

            // Cache and return the territory.
            _territoryCache[settlement] = territory;
            return territory;
        }

        /// <summary>
        /// Checks if a tile has an owner.
        /// </summary>
        /// <param name="tile">The <see cref="int">ID</see> of the world tile to check</param>
        /// <returns>True if <paramref name="tile"/> has an owner; false otherwise</returns>
        public bool AnyFactionOwnsTile(int tile)
        {
            return GetTileOwner(tile)?.Faction != null;
        }

        public bool SettlementHasClaims(Settlement settlement)
        {
            return _territoryCache.ContainsKey(settlement);
        }

        private List<int> _claimsExposeList1;
        private List<TileClaimants> _claimsExposeList2;
        public void ExposeData()
        {
            // DONE!
            Scribe_Collections.Look(ref _claims, "claims", LookMode.Value, LookMode.Deep, ref _claimsExposeList1, ref _claimsExposeList2);

        }

        public class TileClaimants : IExposable
        {
            private Dictionary<Settlement, float> _claims;
            public Dictionary<Settlement, float> Claims
            {
                get => _claims;
                private set => _claims = value;
            }

            public TileClaimants()
            {
                _claims = new Dictionary<Settlement, float>();
            }

            private List<Settlement> _claimsExposeList1;
            private List<float> _claimsExposeList2;
            public void ExposeData()
            {
                // DONE!
                Scribe_Collections.Look(ref _claims, "claims", LookMode.Reference, LookMode.Value, ref _claimsExposeList1, ref _claimsExposeList2);
            }
        }
    }
}