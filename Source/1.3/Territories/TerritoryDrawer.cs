using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Logger = Empire_Rewritten.Utils.Logger;

namespace Empire_Rewritten.Territories
{
    [UsedImplicitly]
    public class TerritoryDrawer : WorldLayer
    {
        public static bool dirty = true;
        [NotNull] private readonly List<Vector3> vertices = new List<Vector3>();

        public override bool ShouldRegenerate => dirty;

        protected override float Alpha => TerritoryUtils.TerritoryAlpha;

        public override IEnumerable Regenerate()
        {
            dirty = false;
            for (int i = subMeshes.Count - 1; i >= 0; i--)
            {
                subMeshes[i].Clear(MeshParts.All);
            }

            DrawAllTerritories();
            FinalizeMesh(MeshParts.All);
            yield break;
        }

        private void DrawTile(int tileId, [NotNull] LayerSubMesh subMesh)
        {
            Find.WorldGrid.GetTileVertices(tileId, vertices);
            int subMeshVerticesCount = subMesh.verts.Count;
            int verticesCount = vertices.Count;
            for (int i = 0; i < verticesCount; i++)
            {
                subMesh.verts.Add(vertices[i] + vertices[i].normalized * 0.012f);
                subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(verticesCount, i) + Vector2.one) / 2f);
                if (i < verticesCount - 2)
                {
                    subMesh.tris.Add(subMeshVerticesCount + i + 2);
                    subMesh.tris.Add(subMeshVerticesCount + i + 1);
                    subMesh.tris.Add(subMeshVerticesCount);
                }
            }
        }

        private void DrawTerritory([NotNull] Territory territory)
        {
            Color factionColor = territory.Faction.Color;

            Material material = MaterialPool.MatFrom("Territory",
                                                     ShaderDatabase.WorldOverlayTransparentLit,
                                                     factionColor,
                                                     WorldMaterials.WorldObjectRenderQueue);
            LayerSubMesh layerSubMesh = GetSubMesh(material);
            foreach (int tile in territory.Tiles)
            {
                DrawTile(tile, layerSubMesh);
            }
        }

        private void DrawAllTerritories()
        {
            foreach (Territory territory in TerritoryManager.CurrentInstance?.Territories ?? Enumerable.Empty<Territory>())
            {
                if (territory == null)
                {
                    Logger.Error("Null territory");
                    continue;
                }

                DrawTerritory(territory);
            }
        }
    }
}
