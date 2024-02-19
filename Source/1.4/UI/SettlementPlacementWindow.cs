using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.UI
{
    class SettlementPlacementWindow : Window
    {
        public override Vector2 InitialSize => new Vector2(350, 650);

        protected override void SetInitialSizeAndPosition()
        {
            float mid = Verse.UI.screenHeight / 2;

            windowRect = new Rect(Verse.UI.screenWidth - InitialSize.x, mid - InitialSize.y / 2, InitialSize.x, InitialSize.y);
        }

        private FlexRect _root;

        public SettlementPlacementWindow()
        {
            doCloseX = true;
            onlyOneOfTypeAllowed = true;
            preventCameraMotion = false;

            _root = new FlexRect("root");
            _root.Top(0.1f, "top");
            _root.Bottom(0.1f, "bottom");
            _root.Center(1f, 0.8f).Top(0.5f, "modifiers");
            _root.Center(1f, 0.8f).Bottom(0.5f, "cost");

        }

        public override void DoWindowContents(Rect inRect)
        {
            // If we aren't in the world/global map, close the window
            if (Find.World == null || Find.World.renderer == null || Find.World.renderer.wantedMode != WorldRenderMode.Planet)
            {
                Find.WindowStack.TryRemove(this);
                return;
            }

            Dictionary<string, Rect> rects = _root.Resolve(inRect);

            // Write header
            Widgets.DrawBox(rects["top"]);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rects["top"], "Empire_SP_Header".Translate());
            WindowHelper.ResetTextAndColor();

            int selected = Find.WorldSelector.selectedTile;
            if (selected == -1)
            {
                // Draw error message
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["bottom"], "Empire_SP_NoTileSelected".Translate());
                WindowHelper.ResetTextAndColor();
                return;
            }
            Tile tile = Find.WorldGrid[selected];

            _DrawModifierTable(rects, tile);

            // Draw divider between tables
            Widgets.DrawLineHorizontal(0, rects["modifiers"].yMax, inRect.width);

            _DrawCostTable(rects);

            string reason = "";
            if (!SettlementDetails.CanBuildAt(Faction.OfPlayer, selected, out reason))
            {
                // Draw error message
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["bottom"], reason);
                WindowHelper.ResetTextAndColor();
                return;
            }

            // Draw confirm button
            if (Widgets.ButtonText(rects["bottom"], "Empire_SP_Settle".Translate()))
            {
                Find.WindowStack.TryRemove(this);

                // Create settlement
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.Tile = selected;
                settlement.SetFaction(Faction.OfPlayer);

                // Create random name
                List<string> used = new List<string>();
                foreach (Settlement found in Find.WorldObjects.Settlements) used.Add(found.Name);
                settlement.Name = NameGenerator.GenerateName(Faction.OfPlayer.def.factionNameMaker, used, true);

                // Add to world
                Find.WorldObjects.Add(settlement);
                EmpireWorldComp.Current.SettlementDetails.Add(settlement, new SettlementDetails(settlement));
                EmpireWorldComp.Current.TerritoryManager.AddClaims(settlement);
            }
        }

        private Vector2 _scrollPosition;
        private List<ResourceModifier> _modifiers;
        private Tile _last;
        private void _DrawModifierTable(Dictionary<string, Rect> rects, Tile tile)
        {

            // Draw header
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rects["modifiers"].TopPartPixels(30f), "Empire_SP_ModifierHeader".Translate());

            List<ResourceDef> resources = DefDatabase<ResourceDef>.AllDefsListForReading;
            float rowHeight = 30f;
            Widgets.BeginScrollView(rects["modifiers"].BottomPartPixels(rects["modifiers"].height - 30f), ref _scrollPosition, new Rect(0, 0, rects["modifiers"].width - 16f, resources.Count * rowHeight));

            if (_modifiers == null || tile != _last)
            {
                _modifiers = new List<ResourceModifier>();
                foreach (ResourceDef d in DefDatabase<ResourceDef>.AllDefs) _modifiers.Add(d.GetTileModifier(tile));
                _last = tile;
            }

            int index = 0;
            foreach (ResourceModifier mod in _modifiers)
            {
                // Draw row with the icon, name, and mod.multiplier
                Rect row = new Rect(0, index * rowHeight, rects["modifiers"].width - 16f, rowHeight);
                if (index % 2 == 1) Widgets.DrawLightHighlight(row);

                // Draw icon
                Rect resIconRect = new Rect(0, index * rowHeight, rowHeight, rowHeight);
                GUI.DrawTexture(resIconRect.ContractedBy(4f), mod.def.IconTexture, ScaleMode.ScaleToFit);

                // Draw name
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rowHeight + 4f, index * rowHeight, rects["modifiers"].width - rowHeight - 4f - 16f, rowHeight).ContractedBy(2f), mod.def.LabelCap);


                // Draw multiplier
                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = mod.multiplier >= 1 ? Color.green : Color.red;
                Widgets.Label(new Rect(rowHeight + 4f, index * rowHeight, rects["modifiers"].width - rowHeight - 4f - 16f, rowHeight).ContractedBy(2f), (mod.multiplier * 100 - 100).ToStringWithSign("##0") + "%");
                GUI.color = Color.white;
                index++;
            }

            Widgets.EndScrollView();
            WindowHelper.ResetTextAndColor();
        }

        private Vector2 _scrollPosition2;
        private void _DrawCostTable(Dictionary<string, Rect> rects)
        {
            // Draw header
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rects["cost"].TopPartPixels(30f), "Empire_SP_CostHeader".Translate());

            Dictionary<ResourceDef, int> costs = SettlementDetails.SettlementCost;
            float rowHeight = 30f;
            Widgets.BeginScrollView(rects["cost"].BottomPartPixels(rects["cost"].height - 30f), ref _scrollPosition2, new Rect(0, 0, rects["cost"].width - 16f, costs.Count * rowHeight));

            int index = 0;
            foreach (ResourceDef resource in costs.Keys)
            {
                // Draw row with the icon, name, and mod.multiplier
                Rect row = new Rect(0, index * rowHeight, rects["cost"].width - 16f, rowHeight);
                if (index % 2 == 1) Widgets.DrawLightHighlight(row);

                // Draw icon
                Rect resIconRect = new Rect(0, index * rowHeight, rowHeight, rowHeight);
                GUI.DrawTexture(resIconRect.ContractedBy(4f), resource.IconTexture, ScaleMode.ScaleToFit);

                // Draw name
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rowHeight + 4f, index * rowHeight, rects["cost"].width - rowHeight - 4f - 16f, rowHeight).ContractedBy(2f), resource.LabelCap);

                // Draw cost
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(new Rect(rowHeight + 4f, index * rowHeight, rects["cost"].width - rowHeight - 4f - 16f, rowHeight).ContractedBy(2f), costs[resource].ToString());

                index++;
            }

            Widgets.EndScrollView();
            WindowHelper.ResetTextAndColor();
        }
    }
}
