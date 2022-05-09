﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Territories
{
    public class Territory : IExposable
    {
        [NotNull] private Faction faction;
        [NotNull] private List<int> tiles = new List<int>();

        [UsedImplicitly]
        public Territory() { }

        public Territory([NotNull] Faction faction)
        {
            this.faction = faction;
        }

        [NotNull] public Faction Faction => faction;

        [NotNull] public List<int> Tiles => tiles;

        public void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref tiles, "tiles");
        }

        public bool HasTile(int tile)
        {
            return tiles.Contains(tile);
        }

        public void ClaimTile(int id)
        {
            if (TerritoryManager.CurrentInstance != null && !TerritoryManager.CurrentInstance.AnyFactionOwnsTile(id))
            {
                tiles.Add(id);
                TerritoryDrawer.dirty = true;
            }
        }

        public void ClaimTiles([NotNull] List<int> ids)
        {
            foreach (int tile in ids)
            {
                ClaimTile(tile);
            }
        }

        public void UnclaimTile(int id)
        {
            if (tiles.Contains(id))
            {
                tiles.Remove(id);
            }
        }

        public async void SettlementClaimTiles([NotNull] Settlement settlement)
        {
            // This could cause a race condition where two Empires claim the same Tile
            await Task.Run(() => ClaimTiles(GetSurroundingTiles(settlement.Tile, (int)(faction.def.techLevel + 1))));
        }

        /// <summary>
        ///     Recursively gets neighboring <see cref="int">Tile IDs</see>.
        /// </summary>
        /// <param name="centerTileId"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [NotNull]
        public static List<int> GetSurroundingTiles(int centerTileId, int distance)
        {
            if (distance <= 0)
            {
                return new List<int> { centerTileId };
            }

            if (distance == 1)
            {
                return TileAndNeighborsClaimable(centerTileId);
            }

            List<int> neighboringTiles = TileAndNeighborsClaimable(centerTileId);
            List<int> result = new List<int>(neighboringTiles);

            int currentDistance = 1;

            foreach (int tile in neighboringTiles)
            {
                if (Find.WorldPathGrid.PassableFast(tile))
                {
                    foreach (int newTileId in GetSurroundingTiles(tile, distance - currentDistance))
                    {
                        if (!result.Contains(newTileId) && Find.WorldPathGrid.PassableFast(newTileId))
                        {
                            result.Add(newTileId);
                        }
                    }

                    currentDistance++;
                }
            }

            return result;
        }

        [NotNull]
        private static List<int> TileAndNeighborsClaimable(int tile)
        {
            List<int> result = TileAndNeighbors(tile);
            result.RemoveAll(tileID => !Find.WorldPathGrid.PassableFast(tileID));
            return result;
        }

        [NotNull]
        private static List<int> TileAndNeighbors(int tile)
        {
            List<int> result = new List<int>();
            Find.WorldGrid.GetTileNeighbors(tile, result);
            result.Add(tile);
            return result;
        }
    }
}
