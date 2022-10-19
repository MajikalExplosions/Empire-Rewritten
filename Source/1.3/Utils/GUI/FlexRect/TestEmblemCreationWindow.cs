using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Empire_Rewritten.Emblems;
using System.Net;
using RimWorld;

namespace Empire_Rewritten.Utils
{
    public class TestEmblemCreationWindow : Window
    {
        //TODO Do we want something like this?
        public override Vector2 InitialSize => new Vector2(800, 600);
        private const int CUSTOM_COLOR = 100;
        private const int GRID_CELL_WIDTH = 80;
        private FlexRect root;

        private Emblem Emblem;
        private bool IconPanelOpen, PatternPanelOpen, ShapePanelOpen;

        public TestEmblemCreationWindow()
        {
            forcePause = true;
            preventCameraMotion = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
            root = new FlexRect("root");
            root.BottomLeft(0.45f, 0.8f, "preview");
            root.TopLeft(0.45f, 0.2f, "headerBar");

            FlexRect settings = root.Right(0.55f);
            settings.BottomRight(0.5f, 0.1f, "save");
            settings.BottomLeft(0.5f, 0.1f, "discard");

            FlexRect pattern = settings.Top(0.9f).Top(0.5f, "patternPanel");
            FlexRect color = settings.Top(0.9f).Bottom(0.5f, "colorPanel");
            settings.Top(0.9f, "settingsPanel");

            pattern.Grid(3, 1, 0, 0, "patternIcon");
            pattern.Grid(3, 1, 1, 0, "patternBack");
            pattern.Grid(3, 1, 2, 0, "patternShape");
            color.Grid(3, 1, 0, 0, "colorIcon");
            color.Grid(3, 1, 1, 0, "colorBack");
            color.Grid(3, 1, 2, 0, "colorAccent");

            root.MergeChildren();

            //TODO Use an actual emblem, not a new one.
            Emblem = new Emblem();
            Emblem.Icon = DefDatabase<EmblemPartDef>.GetNamed("icon_cross");
            Emblem.Pattern = DefDatabase<EmblemPartDef>.GetNamed("pattern_hstripe2");
            Emblem.Shape = DefDatabase<EmblemPartDef>.GetNamed("shape_round");
            IconPanelOpen = false;
            PatternPanelOpen = false;
            ShapePanelOpen = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Dictionary<string, Rect> rects = root.Resolve(inRect);
            WindowHelper.ResetTextAndColor();

            Widgets.DrawBox(rects["preview"].ContractedBy(4f), thickness: 2);

            bool save = Widgets.ButtonText(rects["save"].ContractedBy(4f), "Save Changes");
            bool close = Widgets.ButtonText(rects["discard"].ContractedBy(4f), "Discard Changes");

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(rects["headerBar"], "EMPIRE NAME HERE");
            Text.Anchor = TextAnchor.UpperLeft;

            DrawPreview(rects);
            if (!IconPanelOpen && !PatternPanelOpen && !ShapePanelOpen) DrawCustomization(rects);
            else DrawIconSelection(rects);

            WindowHelper.ResetTextAndColor();
            if (save) this.Close();//TODO SET THE EMBLEM!
            if (close) this.Close();
        }

        private void DrawPreview(Dictionary<string, Rect> rects)
        {
            Widgets.DrawTextureFitted(rects["preview"], Emblem.GetTexture(), 0.85f);
        }

        private void DrawCustomization(Dictionary<string, Rect> rects)
        {

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;

            Widgets.DrawBox(rects["patternPanel"].ContractedBy(4f), thickness: 2);
            Widgets.DrawBox(rects["colorPanel"].ContractedBy(4f), thickness: 2);


            float height = rects["colorIcon"].ContractedBy(8f).height;

            //Labels
            float labelHeight = 50f;
            Widgets.Label(rects["patternIcon"].ContractedBy(4f).TopPartPixels(labelHeight), "Icon");
            Widgets.Label(rects["patternBack"].ContractedBy(4f).TopPartPixels(labelHeight), "Background\nPattern");
            Widgets.Label(rects["patternShape"].ContractedBy(4f).TopPartPixels(labelHeight), "Shape");

            Widgets.Label(rects["colorIcon"].ContractedBy(4f).TopPartPixels(labelHeight), "Icon Color");
            Widgets.Label(rects["colorBack"].ContractedBy(4f).TopPartPixels(labelHeight), "Background\nColor");
            Widgets.Label(rects["colorAccent"].ContractedBy(4f).TopPartPixels(labelHeight), "Accent Color");

            //Pattern

            int pi = GetPartButton(rects["patternIcon"].ContractedBy(8f + 4f).BottomPartPixels(height - labelHeight), Emblem.Icon);
            int pb = GetPartButton(rects["patternBack"].ContractedBy(8f + 4f).BottomPartPixels(height - labelHeight), Emblem.Pattern);
            int ps = GetPartButton(rects["patternShape"].ContractedBy(8f + 4f).BottomPartPixels(height - labelHeight), Emblem.Shape);

            //Color
            int ic = GetColorPalette(rects["colorIcon"].ContractedBy(8f).BottomPartPixels(height - labelHeight), Emblem.IconColor);
            int bc = GetColorPalette(rects["colorBack"].ContractedBy(8f).BottomPartPixels(height - labelHeight), Emblem.BackColor);
            int ac = GetColorPalette(rects["colorAccent"].ContractedBy(8f).BottomPartPixels(height - labelHeight), Emblem.AccentColor);

            if (ic == CUSTOM_COLOR) ;
            else if (ic > -1) Emblem.IconColor = Emblem.PresetColors[ic / Emblem.PresetColors.GetLength(1), ic % Emblem.PresetColors.GetLength(1)];

            if (bc == CUSTOM_COLOR) ;
            else if (bc > -1) Emblem.BackColor = Emblem.PresetColors[bc / Emblem.PresetColors.GetLength(1), bc % Emblem.PresetColors.GetLength(1)];

            if (ac == CUSTOM_COLOR) ;
            else if (ac > -1) Emblem.AccentColor = Emblem.PresetColors[ac / Emblem.PresetColors.GetLength(1), ac % Emblem.PresetColors.GetLength(1)];

            if (pi == 1) IconPanelOpen = true;
            if (pb == 1) PatternPanelOpen = true;
            if (ps == 1) ShapePanelOpen = true;
        }

        private void DrawIconSelection(Dictionary<string, Rect> rects)
        {

            //TODO Use a scroll box
            Rect rect = rects["settingsPanel"];
            if (IconPanelOpen)
            {
                EmblemPartDef part = GetIconPalette(rect, Emblem.Icon, EmblemPartCategory.Icon);
                if (part != null)
                {
                    IconPanelOpen = false;
                    Emblem.Icon = part;
                }
            }
            else if (PatternPanelOpen)
            {
                EmblemPartDef part = GetIconPalette(rect, Emblem.Pattern, EmblemPartCategory.Pattern);
                if (part != null)
                {
                    PatternPanelOpen = false;
                    Emblem.Pattern = part;
                }
            }
            else if (ShapePanelOpen)
            {
                EmblemPartDef part = GetIconPalette(rect, Emblem.Shape, EmblemPartCategory.Shape);
                if (part != null)
                {
                    ShapePanelOpen = false;
                    Emblem.Shape = part;
                }
            }
        }

        private int GetPartButton(Rect r, EmblemPartDef highlight)
        {
            Widgets.DrawBox(r);
            Widgets.DrawHighlightIfMouseover(r);
            Widgets.DrawTextureFitted(r, highlight.Texture, 0.75f);
            return Widgets.ButtonInvisible(r) ? 1 : -1;
        }

        private int GetColorPalette(Rect rect, Color highlight)
        {
            //Layout
            FlexRect root = new FlexRect();

            for (int i = 0; i < Emblem.PresetColors.GetLength(0); i++)
                for (int j = 0; j < Emblem.PresetColors.GetLength(1); j++)
                    root.Grid(Emblem.PresetColors.GetLength(0), Emblem.PresetColors.GetLength(1) + 2, i, j, "grid_" + i.ToString() + "_" + j.ToString());

            root.Bottom(2f / (Emblem.PresetColors.GetLength(1) + 2), "custom");

            Dictionary<string, Rect> r = root.Resolve(rect);

            //Draw & Input
            int selected = -1;
            for (int i = 0; i < Emblem.PresetColors.GetLength(0); i++)
            {
                for (int j = 0; j < Emblem.PresetColors.GetLength(1); j++)
                {
                    Rect colorButton = r["grid_" + i.ToString() + "_" + j.ToString()].ContractedBy(2f);
                    Widgets.DrawBoxSolid(colorButton, Emblem.PresetColors[i, j]);
                    Widgets.DrawHighlightIfMouseover(colorButton);
                    if (highlight == Emblem.PresetColors[i, j]) Widgets.DrawBox(colorButton, 2);
                    if (Widgets.ButtonInvisible(colorButton)) selected = i * Emblem.PresetColors.GetLength(1) + j;
                }
            }

            //TODO Figure out custom color. Which color picker to use?
            //if (Widgets.ButtonText(r["custom"].ContractedBy(2f), "CUSTOM COLOR")) selected = CUSTOM_COLOR;
            return selected;
        }

        private EmblemPartDef GetIconPalette(Rect rect, EmblemPartDef highlight, EmblemPartCategory partCategory)
        {
            //TODO Use a scroll box
            //Calculate grid properties
            int gridWidth = Mathf.FloorToInt(rect.width / GRID_CELL_WIDTH);
            int actualGridSize = Mathf.FloorToInt(rect.width / gridWidth);
            int gridHeight = Mathf.FloorToInt(rect.height / actualGridSize);
            int maxCount = gridWidth * gridHeight;

            EmblemPartDef selected = null;

            //Get all valid parts
            List<EmblemPartDef> parts = Emblem.GetPartsFor(partCategory);
            if (parts.Count > maxCount) Logger.Warn("Too many parts! Grid can't fit them all.");

            //Draw
            for (int row = 0; row < Math.Min(parts.Count / gridWidth + 1, gridHeight); row++)
            {
                //Up to gridWidth, unless we're out of parts to draw.
                for (int col = 0; col < Math.Min(gridWidth, parts.Count - row * gridWidth); col++)
                {
                    Logger.Log(row + " " + col);
                    Rect cell = (new Rect(rect.xMin + col * actualGridSize, rect.yMin + row * actualGridSize, actualGridSize, actualGridSize));
                    cell = cell.ContractedBy(4f);
                    EmblemPartDef part = parts[row * gridWidth + col];
                    Widgets.DrawTextureFitted(cell, part.Texture, 0.8f);
                    Widgets.DrawHighlightIfMouseover(cell);

                    if (highlight == part) Widgets.DrawBox(cell, thickness: 4);
                    else Widgets.DrawBox(cell);

                    if (Widgets.ButtonInvisible(cell)) selected = part;
                }
            }

            return selected;
        }
    }
}
