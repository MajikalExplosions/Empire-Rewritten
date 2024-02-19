using Empire_Rewritten.Resources;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.UI
{
    // Adopted and simplified from Dialog_Trade
    public class ResourceTransferWindow : Window
    {
        private ResourceDef _resourceDef;
        public ResourceDef ResourceDef => _resourceDef;
        private List<ThingEntry> _things;
        private int _maxValue;
        private Action<List<Thing>, ResourceTransferWindow> _onAccept;
        private bool _destroyOnClose;

        private string _title;
        private FlexRect _root;

        public override Vector2 InitialSize => new Vector2(1024f, (float)Verse.UI.screenHeight * 0.75f);

        public ResourceTransferWindow(ResourceDef resource, Action<List<Thing>, ResourceTransferWindow> onAccept, int maxValue = int.MaxValue, string title = "Empire_RT_TitleDefault", bool destroyOnClose = false)
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            onlyOneOfTypeAllowed = true;
            draggable = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;

            _resourceDef = resource;
            _things = new List<ThingEntry>();
            _maxValue = maxValue;
            _onAccept = onAccept;
            _destroyOnClose = destroyOnClose;

            _title = title.Translate();
            _root = new FlexRect("root");
            FlexRect t = _root.Top(0.1f, "title");
            t.Center(0.1f, 1f).Bottom(0.3f, "maxValue");
            _root.Center(1f, 0.8f, "table");

            FlexRect menu = _root.Center(0.5f, 1f).Bottom(0.1f, "menu");
            menu.Left(0.25f, "reset");
            menu.Right(0.25f, "cancel");
            menu.Center(0.5f, 1f, "accept");
        }

        private Vector2 _scrollPosition = Vector2.zero;
        public override void DoWindowContents(Rect inRect)
        {
            Dictionary<string, Rect> rects = _root.Resolve(inRect);

            // Draw title
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rects["title"], _title);
            WindowHelper.ResetTextAndColor();

            if (_maxValue < int.MaxValue)
            {
                // Draw maximum value and current value
                Text.Anchor = TextAnchor.LowerRight;
                Widgets.Label(rects["maxValue"], _maxValue.ToString());
                Text.Anchor = TextAnchor.LowerLeft;
                Widgets.Label(rects["maxValue"], _things.Sum(te => te.GetMarketValue()).ToString());
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(rects["maxValue"], "/");
            }
            else
            {
                // Draw current value
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(rects["maxValue"], _things.Sum(te => te.GetMarketValue()).ToString());
            }
            WindowHelper.ResetTextAndColor();

            // Draw menu
            if (Widgets.ButtonText(rects["reset"], "Empire_RT_Reset".Translate()))
            {
                foreach (ThingEntry entry in _things) entry.Selected = 0;
            }
            if (Widgets.ButtonText(rects["cancel"], "Empire_RT_Cancel".Translate()))
            {
                if (_destroyOnClose)
                {
                    foreach (ThingEntry entry in _things) entry.DestroyAllThings();
                }
                Close();
            }
            // Draw accept button, or replace with label if no things are selected or the total value is too high
            if (_things.Sum(te => te.Selected) == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["accept"], "Empire_RT_NothingSelected".Translate());
            }
            else if (_things.Sum(te => te.GetMarketValue()) > _maxValue)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["accept"], "Empire_RT_TooExpensive".Translate());
            }
            else if (Widgets.ButtonText(rects["accept"], "Empire_RT_Accept".Translate()))
            {
                List<Thing> unselectedThings = new List<Thing>();
                List<Thing> selectedThings = new List<Thing>();
                foreach (ThingEntry te in _things)
                {
                    List<Thing> selected = te.GetSelected(out List<Thing> unselected);
                    selectedThings.AddRange(selected);
                    unselectedThings.AddRange(unselected);
                }

                _onAccept(selectedThings, this);

                // Destroy all unselected things
                if (_destroyOnClose)
                {
                    foreach (ThingEntry entry in _things) entry.DestroyAllThings();
                }
                Close();
            }

            // Draw table
            WindowHelper.ResetTextAndColor();
            float rowHeight = 30f;
            float requestedHeight = (_SelectedEntries() + 1) * rowHeight;
            Rect row;
            Rect controlsRect = new Rect(0, 0, rowHeight * 4, rowHeight);
            Widgets.BeginScrollView(rects["table"], ref _scrollPosition, new Rect(0, 0, rects["table"].width - 16f, requestedHeight));
            rects["table"] = rects["table"].LeftPartPixels(rects["table"].width - 16f);
            // For each selected thing, draw the icon and the name on the left
            //   and up/down buttons, a text box witthe count, and a button to remove on the right
            int i = 0;
            foreach (ThingEntry te in _things)
            {
                if (te.Selected == 0) continue;

                Text.Anchor = TextAnchor.MiddleLeft;
                Thing thing = te.Parent;

                row = new Rect(0, i * rowHeight, rects["table"].width, rowHeight);
                if (i % 2 == 0) Widgets.DrawLightHighlight(row);

                // Draw icon and name
                Widgets.ThingIcon(row.LeftPartPixels(rowHeight).ContractedBy(2f), thing);
                row = row.RightPartPixels(rects["table"].width - rowHeight);
                Widgets.Label(row.LeftPart(0.5f).ContractedBy(2f), thing.LabelNoCount);
                row = row.RightPart(0.5f);

                // Draw down button (all and single), count text box in the middle, and up button (all and single) on the right
                row = row.LeftPartPixels(row.width - rowHeight);
                Rect r = controlsRect.CenteredOnXIn(row).CenteredOnYIn(row);

                Text.Anchor = TextAnchor.MiddleCenter;
                if (te.CanAdd() && Widgets.ButtonText(r.RightPartPixels(rowHeight), ">"))
                {
                    te.Selected += GenUI.CurrentAdjustmentMultiplier();
                }
                if (te.CanSubtract() && Widgets.ButtonText(r.LeftPartPixels(rowHeight), "<"))
                {
                    te.Selected -= GenUI.CurrentAdjustmentMultiplier();
                    if (te.Selected == 0) te.Selected = 1;
                }

                // Draw count text box
                string countString = te.Selected.ToString();
                int tmp = te.Selected;
                Widgets.TextFieldNumeric(r.RightPartPixels(r.width - rowHeight).LeftPartPixels(r.width - rowHeight * 2), ref tmp, ref countString, 1, te.MaxAllowed());
                tmp = Math.Max(1, Math.Min(te.MaxAllowed(), tmp));
                te.Selected = tmp;

                // Draw remove button on the right
                if (Widgets.ButtonText(row.RightPartPixels(rowHeight).ContractedBy(2f), "X"))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    te.Selected = 0;
                }

                i++;
            }

            // Draw the add button at the bottom
            Text.Anchor = TextAnchor.MiddleCenter;
            row = new Rect(0, _SelectedEntries() * rowHeight, rects["table"].width, rowHeight);
            if (Widgets.ButtonText(row, "Empire_RT_Add".Translate()))
            {
                List<ThingEntry> addableThings = new List<ThingEntry>();
                foreach (ThingEntry te in _things)
                {
                    if (te.Selected == 0) addableThings.Add(te);
                }
                // Sort things in selectable by defname, then by thing label
                addableThings.SortBy(t => t.Parent.LabelNoCount);
                addableThings.SortBy(t => t.Parent.def.defName);
                // Check if there are any addable things
                if (addableThings.Count == 0)
                {
                    Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption> { new FloatMenuOption("Empire_RT_NothingToAdd".Translate(), null) }));
                }
                else
                {
                    Find.WindowStack.Add(new FloatMenu(addableThings.Select(t => new FloatMenuOption(t.Parent.LabelNoCount, () => t.Selected = 1, t.Parent.def)).ToList()));
                }
            }
            Widgets.EndScrollView();

            WindowHelper.ResetTextAndColor();
        }

        public bool AddSelectable(Thing thing, bool infinite = false)
        {
            if (!_resourceDef.ThingFilter.Allows(thing)) return false;
            if (infinite) thing.stackCount = int.MaxValue;
            foreach (ThingEntry te in _things)
            {
                if (te.Parent.CanStackWith(thing))
                {
                    te.AddStackable(thing, infinite);
                    return true;
                }
            }

            _things.Add(new ThingEntry(thing, infinite));
            return true;
        }

        private int _SelectedEntries()
        {
            return _things.Sum(te => te.Selected > 0 ? 1 : 0);
        }

        public class ThingEntry
        {
            private Thing _parent;
            private List<Thing> _stackables;
            private int _selected;
            private int _allowed;

            public Thing Parent { get => _parent; }
            public int Selected
            {
                get => _selected;
                set { _selected = value; _selected = Math.Min(_selected, _allowed); }
            }

            public ThingEntry(Thing parent, bool infinite = false)
            {
                _parent = parent;
                _stackables = new List<Thing>() { parent };
                _selected = 0;
                _allowed = infinite ? int.MaxValue : parent.stackCount;
            }

            public bool AddStackable(Thing thing, bool infinite = false)
            {
                if (!thing.CanStackWith(_parent)) return false;
                _stackables.Add(thing);

                // Infinite check
                if (_allowed == int.MaxValue || infinite)
                {
                    _allowed = int.MaxValue;
                    return true;
                }
                else _allowed = thing.stackCount + _allowed;

                return true;
            }

            public float GetMarketValue()
            {
                return _parent.MarketValue * _selected;
            }

            public int MaxAllowed()
            {
                return _allowed;
            }

            public List<Thing> GetSelected(out List<Thing> unselected)
            {
                List<Thing> selected = new List<Thing>();

                if (_selected == 0)
                {
                    unselected = _stackables.ListFullCopy();
                    return selected;
                }
                unselected = new List<Thing>();
                int addedCount = 0;
                foreach (Thing t in _stackables)
                {
                    if (addedCount + t.stackCount <= _selected)
                    {
                        selected.Add(t);
                    }
                    else if (addedCount >= _selected)
                    {
                        // The whole stack is unselected
                        unselected.Add(t);
                    }
                    else
                    {
                        // This occurs when addedCount <= _selected < addedCount + t.stackCount
                        // So we need to split the stack
                        Thing newThing = t.SplitOff(_selected - addedCount);
                        selected.Add(newThing);
                        unselected.Add(t);
                        addedCount += newThing.stackCount;
                    }
                    addedCount += t.stackCount;
                }

                // Log the total stack count of selected things
                return selected;
            }

            public void DestroyAllThings()
            {
                foreach (Thing t in _stackables) t.Destroy();
            }

            public bool CanAdd()
            {
                return _selected < _allowed;
            }

            public bool CanSubtract()
            {
                return _selected > 1;
            }
        }
    }
}
