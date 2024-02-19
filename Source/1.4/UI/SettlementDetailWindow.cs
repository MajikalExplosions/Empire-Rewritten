using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.UI
{
    public class SettlementDetailWindow : Window
    {
        // Find.WindowStack.Add(new SettlementDetailWindow());
        private FlexRect _root;
        private Settlement _settlement;
        private SettlementDetails _details;

        public override Vector2 InitialSize => new Vector2(900, 500);

        public SettlementDetailWindow(Settlement settlement)
        {
            _settlement = settlement;
            _details = _settlement.GetDetails();
            closeOnClickedOutside = true;
            doCloseX = true;
            onlyOneOfTypeAllowed = true;
            draggable = true;

            _root = new FlexRect("root");
            FlexRect header = _root.Top(0.15f, "header");
            header.Left(0.4f, "name");
            FlexRect controls = header.Right(0.6f).Left(0.9f, "controls");
            for (int i = 0; i < 6; i++) controls.Grid(6, 1, i, 0, "controls_" + i);

            FlexRect details = _root.Bottom(0.85f, "details");
            details.Right(0.5f, "resources");
            FlexRect resourceHeader = details.Right(0.5f).Top(0.1f, "resourceHeader");
            resourceHeader.Grid(7, 1, 0, 0, "resourceName");
            resourceHeader.Grid(7, 1, 2, 0, "resourceProdBase");
            resourceHeader.Grid(7, 1, 3, 0, "resourceProdMult");
            resourceHeader.Grid(7, 1, 4, 0, "resourceUpBase");
            resourceHeader.Grid(7, 1, 5, 0, "resourceUpMult");
            resourceHeader.Grid(7, 1, 6, 0, "resourceChange");

            details.Left(0.5f).Top(0.5f, "facilities");
            details.Left(0.5f).Bottom(0.5f, "options");

            _root.MergeChildren();
            _openTicks = 0;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Dictionary<string, Rect> rects = _root.Resolve(inRect);

            _DrawHeader(rects);
            _DrawResources(rects);
            _DrawFacilities(rects);
            // _DrawOptions(rects);

            _openTicks = (_openTicks + 1) % 60;
        }

        private void _DrawHeader(Dictionary<string, Rect> rects)
        {
            Rect name = rects["name"];
            Rect bottom = name.BottomPartPixels(16);
            name = name.TopPartPixels(name.height - 16);

            // Draw progress bar
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            float progress = _details.LevelProgress();
            if (progress >= 0f)
            {

                Widgets.FillableBar(bottom, progress);
                Widgets.Label(bottom, progress.ToStringPercent());
            }
            else Widgets.Label(bottom, "Nothing in progress");

            Rect leftSquare = new Rect(name.x, name.y, name.height, name.height).ContractedBy(4f);
            Rect right = new Rect(name.x + name.height, name.y, name.width - name.height, name.height).ContractedBy(4f);

            // Draw emblem texture
            Texture2D emblem = _settlement.Faction.GetEmblem()?.GetTexture();
            if (emblem != null)
            {
                GUI.DrawTexture(leftSquare, emblem, ScaleMode.ScaleToFit);
            }
            else
            {
                // Draw placeholder
                Widgets.DrawBoxSolid(leftSquare, new Color(1.0f, 0.2f, 0.4f));
            }

            // Write settlement name
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(right, _settlement.Name);

            // Write settlement level
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(right, $"Level {_details.Level}");

            // Reset text
            WindowHelper.ResetTextAndColor();

            // Draw controls on right side
            Rect controls = rects["controls"];
            if (Widgets.ButtonText(rects["controls_5"].TopPartPixels(40).ContractedBy(2f), "Lvl Up"))
            {
                if (_details.CanLevelSettlement())
                {
                    // Show confirmation dialog
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Level Up Settlement?", () => _details.LevelSettlement()));
                }
                else
                {
                    string cost = "";
                    foreach (KeyValuePair<ResourceDef, int> kvp in SettlementDetails.SettlementCost)
                    {
                        cost += $"{kvp.Value} x {kvp.Key.LabelCap}, ";
                    }
                    cost = cost.Substring(0, cost.Length - 2);

                    Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
                    {
                        new FloatMenuOption($"Cannot Level Settlement (req. {cost})", null)
                    }));
                }
            }
            /*
			if (Widgets.ButtonText(rects["controls_4"].TopPartPixels(40).ContractedBy(2f), "Abndn", active: false))
			{
				// TODO
				Logger.Log("Somehow abandoned");
			}
			*/

            WindowHelper.ResetTextAndColor();
        }

        private Vector2 _scrollPosition;
        private int _openTicks;
        private List<ResourceModifier> _prod, _upkeep;
        private List<ResourceDef> _unique;
        private void _DrawResources(Dictionary<string, Rect> rects)
        {
            // Draw header
            Widgets.DrawLightHighlight(rects["resourceHeader"].LeftPartPixels(rects["resourceHeader"].width - 16f));
            Widgets.DrawLineHorizontal(rects["resourceHeader"].x, rects["resourceHeader"].yMax, rects["resourceHeader"].width - 16f);


            Text.Anchor = TextAnchor.MiddleLeft;
            Rect r = rects["resourceName"];
            r.xMax += r.width;
            Widgets.Label(r.ContractedBy(2f), "Resource");
            Widgets.Label(rects["resourceProdBase"].ContractedBy(2f), "Prod.");
            Widgets.Label(rects["resourceProdMult"].ContractedBy(2f), "Prod. Mod");
            Widgets.Label(rects["resourceUpBase"].ContractedBy(2f), "Upkeep");
            Widgets.Label(rects["resourceUpMult"].ContractedBy(2f), "Upkeep Mod");
            Widgets.Label(rects["resourceChange"].ContractedBy(2f), "Daily Total");
            WindowHelper.ResetTextAndColor();

            // Draw Table
            Rect tableRect = rects["resources"].BottomPartPixels(rects["resources"].height - rects["resourceHeader"].height);
            int rowHeight = 30;

            // We can't update every frame b/c it's quite expensive.
            if (_openTicks == 0 || _prod == null || _upkeep == null)
            {
                _prod = _details.GetProduction();
                _upkeep = _details.GetUpkeep();
                _unique = new List<ResourceDef>();

                // Compute number of unique resources in _prod and _upkeep
                foreach (ResourceModifier mod in _prod)
                {
                    if (!_unique.Contains(mod.def))
                    {
                        _unique.Add(mod.def);
                    }
                }
                foreach (ResourceModifier mod in _upkeep)
                {
                    if (!_unique.Contains(mod.def))
                    {
                        _unique.Add(mod.def);
                    }
                }
            }

            float requestedHeight = _unique.Count * rowHeight;
            Widgets.BeginScrollView(tableRect, ref _scrollPosition, new Rect(0, 0, tableRect.width - 16f, requestedHeight));

            int index = 0;
            foreach (ResourceDef resource in _unique)
            {
                float prod = _prod.Find(x => x.def == resource)?.offset ?? 0f;
                float upkeep = _upkeep.Find(x => x.def == resource)?.offset ?? 0f;
                float prodMult = _details.GetProductionMultiplierFor(resource);
                float upkeepMult = _details.GetUpkeepMultiplierFor(resource);
                float change = prod * prodMult - upkeep * upkeepMult;
                if (prod == 0 && upkeep == 0) continue;

                // Create a row for this settlement.
                Rect row = new Rect(0, index * rowHeight, tableRect.width - 16f, rowHeight);
                if (index % 2 == 1) Widgets.DrawLightHighlight(row);

                // Draw row entries
                Text.Anchor = TextAnchor.MiddleLeft;
                float gridWidth = rects["resourceName"].width;

                Rect resIconRect = new Rect(gridWidth * 0, index * rowHeight, rowHeight, rowHeight);
                GUI.DrawTexture(resIconRect.ContractedBy(4f), resource.IconTexture, ScaleMode.ScaleToFit);
                Widgets.Label(new Rect(gridWidth * 0 + rowHeight + 4f, index * rowHeight, gridWidth * 2 - rowHeight - 4f, rowHeight).ContractedBy(2f), resource.LabelCap);
                Widgets.Label(new Rect(gridWidth * 2, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), Mathf.RoundToInt(prod).ToString());
                Widgets.Label(new Rect(gridWidth * 3, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), (prodMult * 100 - 100).ToStringWithSign("##0") + "%");
                Widgets.Label(new Rect(gridWidth * 4, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), Mathf.RoundToInt(upkeep).ToString());
                Widgets.Label(new Rect(gridWidth * 5, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), (upkeepMult * 100 - 100).ToStringWithSign("##0") + "%");
                GUI.color = change >= 0 ? Color.green : Color.red;
                Widgets.Label(new Rect(gridWidth * 6, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), Mathf.RoundToInt(change).ToString());
                WindowHelper.ResetTextAndColor();

                index++;
            }

            Widgets.EndScrollView();
            WindowHelper.ResetTextAndColor();
        }

        private Vector2 _scrollPosition2;
        private void _DrawFacilities(Dictionary<string, Rect> rects)
        {
            //rects["facilities"] = rects["facilities"].BottomPartPixels(rects["facilities"].height - 16f);

            // Draw bounding outline
            Widgets.DrawBox(rects["facilities"], 2);
            rects["facilities"] = rects["facilities"].ContractedBy(2f);

            // Two facilities per column
            float colWidth = (rects["facilities"].width - 16f) / 2f;
            float rowHeight = 80f;
            float requestedHeight = Mathf.CeilToInt(_details.GetFacilities().Count / 2f) * rowHeight;

            Widgets.BeginScrollView(rects["facilities"], ref _scrollPosition2, new Rect(0, 0, rects["facilities"].width - 16f, requestedHeight));

            int index = 0;
            foreach (Facility facility in _details.GetFacilities())
            {
                _DrawFacility(new Rect(index % 2 * colWidth, index / 2 * rowHeight, colWidth, rowHeight).ContractedBy(4f), facility);

                index++;
            }

            if (_details.FacilitySlotsRemaining() != 0)
            {
                _DrawFacility(new Rect(index % 2 * colWidth, index / 2 * rowHeight, colWidth, rowHeight).ContractedBy(4f), null);

                index++;
            }

            Widgets.EndScrollView();
            WindowHelper.ResetTextAndColor();
        }

        private void _DrawFacility(Rect rect, Facility facility)
        {
            rect = rect.ContractedBy(2f);

            // Draw background and line border
            Widgets.DrawLightHighlight(rect);

            // Make button
            if (Widgets.ButtonInvisible(rect))
            {
                string reason = "";
                // If facility is null, show facility selection dropdown.
                if (facility == null)
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (FacilityDef def in DefDatabase<FacilityDef>.AllDefs)
                    {
                        string cost = def.GetCostString();

                        if (_details.CanBuild(def, out reason))
                        {
                            options.Add(new FloatMenuOption(def.LabelCap, delegate
                            {
                                _details.BuildFacility(def);
                            }));
                        }
                        else
                        {
                            options.Add(new FloatMenuOption($"{def.LabelCap} (req. {cost}\n{reason})", null));
                        }

                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                else
                {
                    Find.WindowStack.Add(new FacilityOptionWindow(facility));
                }
            }

            if (facility == null)
            {
                // Write "x Empty Slots" in center
                Text.Anchor = TextAnchor.MiddleCenter;
                int slots = _details.FacilitySlotsRemaining();
                Widgets.Label(rect, $"{slots} Empty Slot" + (slots == 1 ? "" : "s"));
                return;
            }

            WindowHelper.DrawFacilityBox(rect, facility);
        }
    }
}
