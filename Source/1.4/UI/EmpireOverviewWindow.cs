using Empire_Rewritten.Factions;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Logger = Empire_Rewritten.Utils.Logger;

namespace Empire_Rewritten.UI
{
    public class EmpireOverviewWindow : MainTabWindow
    {
        private FlexRect _root;
        private OverviewTab _tab = OverviewTab.Customization;

        public EmpireOverviewWindow() : base()
        {
            _root = new FlexRect("root");

            // Tab menu is on left.
            FlexRect tabMenu = _root.Left(0.15f, "tabMenu");
            tabMenu.SetMinimumSize(new Vector2(0, 200));

            FlexRect content = _root.Right(0.8f, "content");

            // TAB MENU
            tabMenu.Grid(1, 6, 0, 0, "buttonCustomization");
            tabMenu.Grid(1, 6, 0, 1, "buttonSettlements");
            tabMenu.Grid(1, 6, 0, 2, "buttonStockpile");
            tabMenu.Grid(1, 6, 0, 3, "buttonDiplomacy");
            tabMenu.Grid(1, 6, 0, 4, "buttonMilitary");
            tabMenu.Grid(1, 6, 0, 5, "buttonEvents");

            // CREATION TAB
            _root.Center(0.5f, 0.9f).Top(0.2f, "creationName");
            _root.Center(0.5f, 0.9f).Bottom(0.8f).Top(0.8f, "creationEmblem");
            _root.Center(0.5f, 0.9f).Bottom(0.8f).Bottom(0.2f, "creationButton");

            // CUSTOMIZATION TAB
            // Header includes emblem, name, and title
            content.Top(0.15f, "customizationHeader");
            // TODO Modifiers, Traits, Ideoligion, Race, etc.

            // SETTLEMENTS TAB
            FlexRect tableHeader = content.Top(0.1f, "tableHeader");
            int gridLength = 5;
            tableHeader.Grid(gridLength, 1, 0, 0, "settlementName");
            tableHeader.Grid(gridLength, 1, 1, 0, "settlementLevel");
            tableHeader.Grid(gridLength, 1, 3, 0, "settlementFacilities");


            // STOCKPILE TAB
            gridLength = 5;
            tableHeader.Grid(gridLength, 1, 0, 0, "stockpileResource");
            tableHeader.Grid(gridLength, 1, 2, 0, "stockpileAmount");
            tableHeader.Grid(gridLength, 1, 3, 0, "stockpileChange");
            tableHeader.Grid(gridLength, 1, 4, 0, "stockpileAdd");


            _root.MergeChildren();
            _openTicks = 0;
        }

        public override Vector2 RequestedTabSize => new Vector2(_GetHeight(), Verse.UI.screenWidth);

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(Verse.UI.screenWidth, _GetHeight());
            }
        }

        public override void DoWindowContents(Rect rect)
        {

            Dictionary<string, Rect> rects = _root.Resolve(rect);

            if (!EmpireWorldComp.Current.GetTrackedFactions().Contains(Faction.OfPlayer))
            {
                _DoCreationTab(rects);
                return;
            }

            _DoTabMenu(rects);

            switch (_tab)
            {
                case OverviewTab.Customization:
                    _DoCustomizationTab(rects);
                    break;
                case OverviewTab.Settlements:
                    _DoSettlementsTab(rects);
                    break;
                case OverviewTab.Stockpile:
                    _DoStockpileTab(rects);
                    break;
                case OverviewTab.Diplomacy:
                    break;
                case OverviewTab.Military:
                    break;
                case OverviewTab.Events:
                    break;
                default:
                    break;
            }

            _openTicks = (_openTicks + 1) % 60;
        }

        private void _DoTabMenu(Dictionary<string, Rect> rects)
        {
            WindowHelper.ResetTextAndColor();

            Text.Font = GameFont.Medium;
            if (_tab == OverviewTab.Creation) return;

            // Draw box around selected tab.
            WindowHelper.DrawBorderAroundRect(rects["button" + _GetSelectedTab()].ContractedBy(2f), 2, maybeColor: new Color(0.8f, 0.8f, 0.8f));

            // Draw tab buttons.
            // TODO Restyle
            if (Widgets.ButtonText(rects["buttonCustomization"].ContractedBy(5f), "Empire_OV_CustomizationTab".Translate()))
            {
                _tab = OverviewTab.Customization;
            }
            if (Widgets.ButtonText(rects["buttonSettlements"].ContractedBy(5f), "Empire_OV_SettlementTab".Translate()))
            {
                _tab = OverviewTab.Settlements;
            }
            if (Widgets.ButtonText(rects["buttonStockpile"].ContractedBy(5f), "Empire_OV_StockpileTab".Translate()))
            {
                _tab = OverviewTab.Stockpile;
            }
            // TODO Extend to include all tabs.

            WindowHelper.ResetTextAndColor();
        }

        private string _empireName = Faction.OfPlayer.Name;
        private string _settlementName = "Settlement Name";
        private Emblem _empireEmblem = Emblem.GetRandomEmblem();
        private void _DoCreationTab(Dictionary<string, Rect> rects)
        {
            // Text box for empire name
            if (_empireName == null) _empireName = "Empire Name";
            Vector2 center = rects["creationName"].center;
            Rect textBoxRect = new Rect(center.x - 100f, center.y - 30f, 200f, 40f);
            _empireName = Widgets.TextField(textBoxRect, _empireName, 64);

            textBoxRect = new Rect(center.x - 100f, center.y + 30f, 200f, 40f);
            _settlementName = Widgets.TextField(textBoxRect, _settlementName, 64);

            // Draw emblem
            Rect emblemRect = rects["creationEmblem"].ContractedBy(10f);
            // Make emblemRect square and centered
            emblemRect = new Rect(emblemRect.center.x - emblemRect.height / 2, emblemRect.center.y - emblemRect.height / 2, emblemRect.height, emblemRect.height);
            emblemRect = emblemRect.ContractedBy(20f);
            if (_empireEmblem != null)
            {
                // Draw emblem texture as button, and randomize if pressed.
                if (Widgets.ButtonImage(emblemRect, _empireEmblem.GetTexture()))
                {
                    _empireEmblem = Emblem.GetRandomEmblem();
                }
            }

            // Create empire button
            bool disable = false;
            if (!NamePlayerFactionDialogUtility.IsValidName(_empireName)) disable = true;
            if (!NamePlayerSettlementDialogUtility.IsValidName(_settlementName)) disable = true;

            if (Widgets.ButtonText(rects["creationButton"].ContractedBy(5f), "Empire_OV_CreateEmpire".Translate(), active: !disable))
            {
                Logger.Message("Creating player empire " + _empireName);

                // Setup names
                NamePlayerFactionDialogUtility.Named(_empireName);
                // Get the player's first settlement (I think this works?) and rename it.
                Settlement s = Find.WorldObjects.Settlements.FirstOrDefault((Settlement x) => x.Faction == Faction.OfPlayer);
                NamePlayerSettlementDialogUtility.Named(s, _settlementName);

                // Setup faction data
                EmpireWorldComp _wc = EmpireWorldComp.Current;
                if (!_wc.PlayerController.Players.ContainsKey(Faction.OfPlayer)) _wc.PlayerController.CreateUserPlayer();

                _wc.TaxDetails = new Dictionary<ResourceDef, EmpireWorldComp.TaxList>();
                _wc.FactionDetails[Faction.OfPlayer] = new FactionDetails(Faction.OfPlayer, _empireEmblem);

                // Go through all player settlements and add them to the settlement manager.
                //   It's rare, but sometimes the player settles a second before starting an empire.
                foreach (Settlement settlement in Find.WorldObjects.Settlements)
                {
                    if (settlement.Faction == Faction.OfPlayer)
                    {
                        _wc.SettlementDetails.Add(settlement, new SettlementDetails(settlement));
                        _wc.TerritoryManager.AddClaims(settlement);
                    }
                }

                _tab = OverviewTab.Customization;
            }
        }

        private void _DoCustomizationTab(Dictionary<string, Rect> rects)
        {
            Rect header = rects["customizationHeader"];
            Rect leftSquare = new Rect(header.x, header.y, header.height, header.height);
            Rect right = new Rect(header.x + header.height, header.y, header.width - header.height, header.height).ContractedBy(4f);

            // Draw emblem texture
            Texture2D emblem = Faction.OfPlayer.GetEmblem()?.GetTexture();
            if (emblem != null)
            {
                GUI.DrawTexture(leftSquare, emblem, ScaleMode.ScaleToFit);
            }
            else
            {
                // Draw placeholder
                Widgets.DrawBoxSolid(leftSquare, new Color(1.0f, 0.2f, 0.4f));
            }

            // Write faction name
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(right, Faction.OfPlayer.Name);

            // Write faction title
            Text.Font = GameFont.Small;
            if (Verse.UI.screenHeight > 600) Text.Anchor = TextAnchor.LowerLeft;
            else Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(right, "A Player Faction");// TODO Change to an actual title

            // Reset text
            WindowHelper.ResetTextAndColor();
        }

        private Vector2 _scrollPosition;
        private void _DoSettlementsTab(Dictionary<string, Rect> rects)
        {
            if (!EmpireWorldComp.Current.GetTrackedFactions().Contains(Faction.OfPlayer)) return;

            // Draw header
            Widgets.DrawLightHighlight(rects["tableHeader"].LeftPartPixels(rects["tableHeader"].width - 16f));
            Widgets.DrawLineHorizontal(rects["tableHeader"].x, rects["tableHeader"].yMax, rects["tableHeader"].width - 16f);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rects["settlementName"].ContractedBy(2f), "Empire_OV_SettlementTableName".Translate());
            Widgets.Label(rects["settlementLevel"].ContractedBy(2f), "Empire_OV_SettlementTableLevel".Translate());
            Widgets.Label(rects["settlementFacilities"].ContractedBy(2f), "Empire_OV_SettlementTableFacilities".Translate());
            WindowHelper.ResetTextAndColor();

            // Draw Table
            Rect tableRect = rects["content"].BottomPartPixels(rects["content"].height - rects["tableHeader"].height);
            int rowHeight = 30;

            int numSettlements = Faction.OfPlayer.GetTrackedSettlements().Count;
            //int numSettlements = EmpireWorldComp.Current.GetTrackedSettlements().Count;
            float requestedHeight = numSettlements * rowHeight + rowHeight * 2;// We add a button at the bottom.
            Widgets.BeginScrollView(tableRect, ref _scrollPosition, new Rect(0, 0, tableRect.width - 16f, requestedHeight));

            int index = 0;
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (Settlement settlement in Faction.OfPlayer.GetTrackedSettlements())
            {
                // Create a row for this settlement.
                Rect row = new Rect(0, index * rowHeight, tableRect.width - 16f, rowHeight);
                if (index % 2 == 1) Widgets.DrawLightHighlight(row);

                // Draw row entries
                float gridWidth = rects["settlementName"].width;
                Widgets.Label(new Rect(gridWidth * 0, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), settlement.Name);
                Widgets.Label(new Rect(gridWidth * 1, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), settlement.GetDetails().Level.ToString());

                int fIndex = 0;
                foreach (Facility facility in settlement.GetDetails().GetFacilities())
                {
                    Texture texture = facility.def.IconTexture;
                    if (texture == null) continue;
                    Rect iconRect = new Rect(gridWidth * 5 + rowHeight * fIndex, index * rowHeight, rowHeight, rowHeight);

                    // Don't draw icons that would be off the screen.
                    if (iconRect.yMax > tableRect.yMax - 16f) continue;

                    GUI.DrawTexture(iconRect.ContractedBy(4f), texture, ScaleMode.ScaleToFit);
                    fIndex++;
                }

                // Make row clickable
                if (Widgets.ButtonInvisible(row))
                {
                    Find.WindowStack.Add(new SettlementDetailWindow(settlement));
                }

                index++;
            }

            // Add button to create new settlement
            Rect buttonRect = new Rect(0, index * rowHeight, tableRect.width - 16f, rowHeight * 2);
            if (Widgets.ButtonText(buttonRect, "Empire_OV_CreateSettlement".Translate()))
            {
                CameraJumper.TryShowWorld();
                Find.WindowStack.Add(new SettlementPlacementWindow());
                // Close this window
                Find.WindowStack.TryRemove(this);
            }

            Widgets.EndScrollView();
            WindowHelper.ResetTextAndColor();
        }


        private Dictionary<ResourceDef, float> _stockpile;
        private Dictionary<ResourceDef, float> _change;
        private int _openTicks;
        private void _DoStockpileTab(Dictionary<string, Rect> rects)
        {
            if (!EmpireWorldComp.Current.GetTrackedFactions().Contains(Faction.OfPlayer)) return;
            // Draw header
            Widgets.DrawLightHighlight(rects["tableHeader"].LeftPartPixels(rects["tableHeader"].width - 16f));
            Widgets.DrawLineHorizontal(rects["tableHeader"].x, rects["tableHeader"].yMax, rects["tableHeader"].width - 16f);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rects["stockpileResource"].ContractedBy(2f), "Empire_OV_StockpileTableResource".Translate());
            Widgets.Label(rects["stockpileAmount"].ContractedBy(2f), "Empire_OV_StockpileTableAmount".Translate());
            Widgets.Label(rects["stockpileChange"].ContractedBy(2f), "Empire_OV_StockpileTableChange".Translate());
            WindowHelper.ResetTextAndColor();

            // Draw Table
            Rect tableRect = rects["content"].BottomPartPixels(rects["content"].height - rects["tableHeader"].height);
            int rowHeight = 30;

            int numResources = Faction.OfPlayer.GetStockpile().Count;
            //int numSettlements = EmpireWorldComp.Current.GetTrackedSettlements().Count;
            float requestedHeight = numResources * rowHeight;
            Widgets.BeginScrollView(tableRect, ref _scrollPosition, new Rect(0, 0, tableRect.width - 16f, requestedHeight));

            int index = 0;

            // We can't update every frame b/c it's quite expensive.
            if (_openTicks == 0 || _stockpile == null || _change == null)
            {
                _stockpile = Faction.OfPlayer.GetStockpile();
                _change = Faction.OfPlayer.GetResourceChange();
            }

            foreach (ResourceDef resource in DefDatabase<ResourceDef>.AllDefs)
            {
                float stock = _stockpile.ContainsKey(resource) ? _stockpile[resource] : 0;
                float change = _change.ContainsKey(resource) ? _change[resource] : 0;
                if (stock == 0 && change == 0 && !resource.alwaysShowInList) continue;

                // Create a row.
                Rect row = new Rect(0, index * rowHeight, tableRect.width - 16f, rowHeight);
                if (index % 2 == 1) Widgets.DrawLightHighlight(row);

                // Draw row entries
                Text.Anchor = TextAnchor.MiddleLeft;
                float gridWidth = rects["stockpileResource"].width;
                Rect resIconRect = new Rect(gridWidth * 0, index * rowHeight, rowHeight, rowHeight);
                GUI.DrawTexture(resIconRect.ContractedBy(4f), resource.IconTexture, ScaleMode.ScaleToFit);
                Widgets.Label(new Rect(gridWidth * 0 + rowHeight + 4f, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), resource.LabelCap);
                Widgets.Label(new Rect(gridWidth * 2, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), Mathf.RoundToInt(stock).ToString());
                GUI.color = change >= 0 ? Color.green : Color.red;
                Widgets.Label(new Rect(gridWidth * 3, index * rowHeight, gridWidth, rowHeight).ContractedBy(2f), Mathf.RoundToInt(change).ToString());
                WindowHelper.ResetTextAndColor();

                Text.Anchor = TextAnchor.MiddleCenter;

                if (Find.CurrentMap != null)
                {
                    if (Widgets.ButtonText(new Rect(gridWidth * 4, index * rowHeight, (gridWidth - 16f) / 2f, rowHeight).ContractedBy(2f), "Empire_OV_StockpileTableAddButton".Translate()))
                    {
                        ResourceTransferWindow w = new ResourceTransferWindow(resource, OnTransferAcceptAdd, title: "Empire_RT_AddHeader");

                        foreach (Thing t in Find.CurrentMap.listerThings.AllThings.Where(t => resource.ThingFilter.Allows(t)))
                            w.AddSelectable(t);

                        if (ModsConfig.BiotechActive)
                        {
                            foreach (Building thing in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.GeneBank))
                            {
                                CompGenepackContainer comp = thing.TryGetComp<CompGenepackContainer>();
                                if (comp != null)
                                {
                                    foreach (Thing containedGenepack in comp.ContainedGenepacks.Where(t => resource.ThingFilter.Allows(t)))
                                        w.AddSelectable(containedGenepack);
                                }
                            }
                        }

                        Find.WindowStack.Add(w);
                    }
                    else if (Widgets.ButtonText(new Rect(gridWidth * 4 + (gridWidth - 16f) / 2f, index * rowHeight, (gridWidth - 16f) / 2f, rowHeight).ContractedBy(2f), "Empire_OV_StockpileTableTaxButton".Translate()))
                    {
                        int maxValue = (int)EmpireWorldComp.Current.GetMaxTax(resource);
                        ResourceTransferWindow w = new ResourceTransferWindow(resource, OnTaxAccept, maxValue: maxValue, title: "Empire_RT_TaxHeader", destroyOnClose: true);
                        foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(td => resource.ThingFilter.Allows(td)))
                        {
                            Thing thing = null;
                            if (thingDef.MadeFromStuff)
                            {
                                foreach (ThingDef stuff in GenStuff.AllowedStuffsFor(thingDef))
                                {
                                    thing = ThingMaker.MakeThing(thingDef, stuff);
                                    w.AddSelectable(thing, infinite: true);
                                }
                            }
                            else
                            {
                                thing = ThingMaker.MakeThing(thingDef);
                                w.AddSelectable(thing, infinite: true);
                            }
                        }
                        Find.WindowStack.Add(w);
                    }
                }

                index++;
            }

            Widgets.EndScrollView();
            WindowHelper.ResetTextAndColor();
        }

        private void OnTransferAcceptAdd(List<Thing> items, ResourceTransferWindow w)
        {
            foreach (Thing thing in items)
            {
                Logger.Message("Transferring " + thing.LabelCapNoCount + " x " + thing.stackCount);
                float totalValue = thing.MarketValue * thing.stackCount;

                // Add value to relevant resource in stockpile
                float cur = 0;
                Dictionary<ResourceDef, float> stockpile = Faction.OfPlayer.GetStockpile();
                if (stockpile.ContainsKey(w.ResourceDef)) cur = stockpile[w.ResourceDef];
                stockpile[w.ResourceDef] = cur + totalValue;

                // Remove from map
                thing.Destroy();
            }
        }

        private void OnTaxAccept(List<Thing> items, ResourceTransferWindow w)
        {
            if (!EmpireWorldComp.Current.TaxDetails.ContainsKey(w.ResourceDef))
                EmpireWorldComp.Current.TaxDetails[w.ResourceDef] = new EmpireWorldComp.TaxList();
            else EmpireWorldComp.Current.TaxDetails[w.ResourceDef].Things.Clear();
            foreach (Thing thing in items)
            {
                EmpireWorldComp.Current.TaxDetails[w.ResourceDef].Things.Add(thing);
            }
        }

        private string _GetSelectedTab()
        {
            switch (_tab)
            {
                case OverviewTab.Customization:
                    return "Customization";
                case OverviewTab.Settlements:
                    return "Settlements";
                case OverviewTab.Stockpile:
                    return "Stockpile";
                case OverviewTab.Diplomacy:
                    return "Diplomacy";
                case OverviewTab.Military:
                    return "Military";
                case OverviewTab.Events:
                    return "Events";
                default:
                    return "Customization";
            }
        }

        private int _GetHeight()
        {
            int maxHeight = Verse.UI.screenHeight - 35;
            int minHeight = 400;
            return Math.Min(Math.Max(minHeight, Verse.UI.screenHeight * 2 / 3), maxHeight);
        }

        private enum OverviewTab
        {
            Creation,
            Customization,
            Settlements,
            Stockpile,
            Diplomacy,
            Military,
            Events
        }
    }
}
