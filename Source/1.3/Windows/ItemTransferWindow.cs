using System;
using System.Collections.Generic;
using Empire_Rewritten.Controllers;
using Empire_Rewritten.Settlements;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Empire_Rewritten.Windows
{
    public class ItemTransferWindow : Window
    {
        private const string TradeArrowPath = "UI/Widgets/TradeArrow";

        [NotNull] private static readonly Texture2D TradeArrow =
            ContentFinder<Texture2D>.Get(TradeArrowPath) ?? throw new NullReferenceException("Could not find texture " + TradeArrowPath);

        [NotNull] private readonly Dictionary<ThingDef, int> combinedItems = new Dictionary<ThingDef, int>();
        [NotNull] private readonly Dictionary<ThingDef, int> playerItems = new Dictionary<ThingDef, int>();
        [NotNull] private readonly Dictionary<ThingDef, int> transferAmounts = new Dictionary<ThingDef, int>();
        [NotNull] private readonly Dictionary<ThingDef, string> transferBuffer = new Dictionary<ThingDef, string>();

        [NotNull] private readonly Empire playerEmpire;
        [NotNull] private readonly Map playerMap;

        private readonly Rect rectAmountOnMapDesc;
        private readonly Rect rectBot;

        private readonly Rect rectButtonApply;

        private readonly Rect rectFull = new Rect(0f, 0f, 900f, 600f);
        private readonly Rect rectItem;

        private readonly Rect rectItemTransferFull;
        private readonly Rect rectItemTransferInner;
        private readonly Rect rectItemTransferOuter;
        private readonly Rect rectItemTransferTop;

        private readonly Rect rectItemTransferTopBottom;
        private readonly Rect rectLabelDesc;
        private readonly Rect rectMain;
        private readonly Rect rectMid;

        private readonly Rect rectRest;
        private readonly Rect rectRestLeft;
        private readonly Rect rectRestRight;

        private readonly Rect rectStorageItem;
        private readonly Rect rectStorageManagerListFull;
        private readonly Rect rectStorageManagerListInner;
        private readonly Rect rectStorageManagerListOuter;
        private readonly Rect rectStorageManagerListTop;

        private readonly Rect rectTop;

        private Vector2 itemTransferScroll;
        private Vector2 storageManagerScroll;

        public ItemTransferWindow()
        {
            doCloseX = true;
            onlyOneOfTypeAllowed = true;
            preventCameraMotion = false;
            forcePause = true;

            if (Find.CurrentMap?.IsPlayerHome == true)
            {
                playerMap = Find.CurrentMap;
            }
            else
            {
                playerMap = Find.AnyPlayerHomeMap ?? throw new ArgumentNullException(nameof(playerMap), "No player home map found");
            }

            playerEmpire = UpdateController.CurrentWorldInstance?.FactionController?.ReadOnlyFactionSettlementData
                                           .Find(settlementData => settlementData?.Empire.IsAIPlayer == false)
                                           ?.Empire ??
                           throw new ArgumentNullException(nameof(playerEmpire), "No player empire found");

            GetMapItems();
            CombineItemDicts();

            rectMain = rectFull.ContractedBy(25f);
            rectTop = rectMain.TopPartPixels(30f);
            rectBot = rectMain.BottomPartPixels(30f);
            rectButtonApply = new Rect(rectBot.x, rectBot.y + 5f, rectBot.width, rectBot.height - 10f);
            rectMid = new Rect(rectMain.x, rectMain.y + 30f, rectMain.width, rectMain.height - 30f * 2f);

            rectStorageManagerListFull = rectMid.LeftPartPixels(240);
            rectStorageManagerListTop = rectStorageManagerListFull.TopPartPixels(33f);
            rectStorageManagerListOuter = rectStorageManagerListFull.BottomPartPixels(rectStorageManagerListFull.height - rectStorageManagerListTop.height - 5f)
                                                                    .MoveRect(new Vector2(0f, -5f));
            rectStorageManagerListInner = rectStorageManagerListOuter.GetInnerScrollRect(playerEmpire.StorageTracker.StoredThings.Count);
            rectStorageItem = rectStorageManagerListInner.TopPartPixels(29f);

            rectItemTransferFull = rectMid.RightPartPixels(rectMid.width - rectStorageManagerListOuter.width - 5f);
            rectItemTransferTop = rectItemTransferFull.TopPartPixels(33f);
            rectItemTransferOuter = rectItemTransferFull.BottomPartPixels(rectItemTransferFull.height - rectItemTransferTop.height - 5f)
                                                        .MoveRect(new Vector2(0f, -5f));
            rectItemTransferInner = rectItemTransferOuter.GetInnerScrollRect(combinedItems.Count * 29f);
            rectItem = rectItemTransferInner.TopPartPixels(29f);

            rectItemTransferTopBottom = rectItemTransferTop.BottomPartPixels(28f);
            rectLabelDesc = rectItemTransferTopBottom.LeftPartPixels(230f);
            rectAmountOnMapDesc = rectItemTransferTopBottom.RightPartPixels(rectItemTransferTopBottom.width - rectLabelDesc.width - 5f).LeftPartPixels(60f);
            rectRest = rectItemTransferTopBottom.RightPartPixels(rectItemTransferTopBottom.width - rectLabelDesc.width - 5f - rectAmountOnMapDesc.width - 5f);

            Rect tempRight = rectRest.RightPartPixels(100f);
            rectRestLeft = rectRest.LeftPartPixels(rectRest.width - tempRight.width);
            rectRestRight = new Rect(tempRight.x + 5f, tempRight.y, tempRight.width - 5f, tempRight.height);
        }

        protected override float Margin => 0f;

        public override Vector2 InitialSize => rectFull.size;

        private void GetMapItems()
        {
            List<Thing> mapItems = playerMap.listerThings.ThingsMatching(new ThingRequest { group = ThingRequestGroup.HaulableEver });

            foreach (Thing item in mapItems)
            {
                if (item.def != null && !playerItems.TryAdd(item.def, item.stackCount)) playerItems[item.def] += item.stackCount;
            }
        }

        private void CombineItemDicts()
        {
            foreach (ThingDef key in playerItems.Keys)
            {
                if (key is null) throw new NullReferenceException("what");
            }

            foreach ((ThingDef thing, int amount) in playerItems)
            {
                if (thing != null && !combinedItems.TryAdd(thing, amount)) combinedItems[thing] += amount;
            }

            foreach ((ThingDef thing, int amount) in playerEmpire.StorageTracker.StoredThings)
            {
                if (thing != null && !combinedItems.TryAdd(thing, amount)) combinedItems[thing] += amount;
            }

            foreach (ThingDef thing in combinedItems.Keys)
            {
                transferAmounts.Add(thing, 0);
                transferBuffer.Add(thing, "0");
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTop();
            DrawBottom();
            DisplayStoredItems();
            DrawTransferTopPart();

            Widgets.BeginScrollView(rectItemTransferOuter, ref itemTransferScroll, rectItemTransferInner);

            //DO THIS
            int count = 0;
            foreach (ThingDef thing in combinedItems.Keys)
            {
                Rect itemRect = rectItem.MoveRect(new Vector2(0f, rectItem.height * count));
                Rect itemRectIcon = itemRect.LeftPartPixels(itemRect.height).ContractedBy(4f);
                Rect itemRectLabel = itemRect.MoveRect(new Vector2(itemRectIcon.width + 5f, 0f));
                itemRectLabel.xMax = rectLabelDesc.xMax;

                Rect itemAmountOnMap = itemRect.AlignXWith(rectAmountOnMapDesc);
                Rect itemRectTransferAmount = itemRect.AlignXWith(rectRestLeft);
                Rect itemFullyReduceAmountButton = itemRectTransferAmount.LeftPartPixels(itemRectTransferAmount.height);
                Rect itemFullyIncreaseAmountButton = itemRectTransferAmount.RightPartPixels(itemRectTransferAmount.height);
                Rect itemReduceAmountButton = itemRectTransferAmount.LeftPartPixels(itemRectTransferAmount.height)
                                                                    .MoveRect(new Vector2(itemRectTransferAmount.height, 0));
                Rect itemIncreaseAmountButton = itemRectTransferAmount.RightPartPixels(itemRectTransferAmount.height)
                                                                      .MoveRect(new Vector2(-itemRectTransferAmount.height, 0));
                Rect itemNumericFieldFull = new Rect(itemRectTransferAmount.x + itemRectTransferAmount.height * 2f,
                                                     itemRectTransferAmount.y,
                                                     itemRectTransferAmount.width - itemRectTransferAmount.height * 4f,
                                                     itemRectTransferAmount.height);
                Rect itemNumericFieldTexture = new Rect(0f, 0f, TradeArrow.width, TradeArrow.height).CenteredOnXIn(itemNumericFieldFull)
                                                                                                    .CenteredOnYIn(itemNumericFieldFull);
                Rect itemNumericFieldInput = new Rect(0f, 0f, 55f, 21f).CenteredOnXIn(itemNumericFieldFull).CenteredOnYIn(itemNumericFieldFull);
                Rect itemRectStorageAmount = itemRect.AlignXWith(rectRestRight);
                Rect itemInfoRect = itemRect.RightPartPixels(itemRect.height).ContractedBy(4f);
                itemRectStorageAmount.xMax = itemInfoRect.x - 5f;

                int modifier = GenUI.CurrentAdjustmentMultiplier();
                int itemTransferAmount = transferAmounts[thing];
                int amountOnMap = playerItems.ContainsKey(thing) ? playerItems[thing] : 0;
                int amountInStorage = playerEmpire.StorageTracker.StoredThings.ContainsKey(thing) ? playerEmpire.StorageTracker.StoredThings[thing] : 0;
                string itemTransferAmountBuffer = transferBuffer[thing];

                itemRect.DoRectHighlight(count % 2 == 1);

                Text.Anchor = TextAnchor.MiddleLeft;
                MouseoverSounds.DoRegion(itemRectIcon);
                Widgets.ThingIcon(itemRectIcon, thing);

                Widgets.DrawHighlightIfMouseover(itemRectLabel);
                MouseoverSounds.DoRegion(itemRectLabel);
                Widgets.Label(itemRectLabel.MoveRect(new Vector2(5f, 0f)), thing.LabelCap);
                TooltipHandler.TipRegion(itemRectLabel, thing.description);

                Text.Anchor = TextAnchor.MiddleRight;
                MouseoverSounds.DoRegion(itemAmountOnMap);
                Widgets.DrawHighlightIfMouseover(itemAmountOnMap);
                Widgets.Label(itemAmountOnMap, amountOnMap.ToString());
                TooltipHandler.TipRegion(itemAmountOnMap, "Empire_ITW_AmountOnMap".Translate());

                Text.Anchor = TextAnchor.MiddleRight;
                //MouseoverSounds.DoRegion(itemRectTransferAmount);
                //Widgets.DrawHighlightIfMouseover(itemRectTransferAmount);
                MouseoverSounds.DoRegion(itemNumericFieldInput);
                Widgets.TextFieldNumeric(itemNumericFieldInput, ref itemTransferAmount, ref itemTransferAmountBuffer, -amountInStorage, amountOnMap);

                bool amountIs0OrLess = itemTransferAmount <= 0;
                bool amountIs0OrMore = itemTransferAmount >= 0;
                if (amountOnMap + amountInStorage > 1)
                {
                    itemFullyReduceAmountButton.DrawButtonText(amountIs0OrLess ? "<<" : "0",
                                                               () =>
                                                               {
                                                                   itemTransferAmount = amountIs0OrLess ? -amountInStorage : 0;
                                                                   SoundDefOf.Tick_High.PlayOneShotOnCamera();
                                                               },
                                                               itemTransferAmount == -amountInStorage);

                    itemFullyIncreaseAmountButton.DrawButtonText(amountIs0OrMore ? ">>" : "0",
                                                                 () =>
                                                                 {
                                                                     itemTransferAmount = amountIs0OrMore ? amountOnMap : 0;
                                                                     SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                                                                 },
                                                                 itemTransferAmount == amountOnMap);
                }
                else
                {
                    modifier = 1;
                    itemReduceAmountButton.x -= itemRectTransferAmount.height;
                    itemReduceAmountButton.width += itemRectTransferAmount.height;
                    itemIncreaseAmountButton.width += itemRectTransferAmount.height;
                }

                itemReduceAmountButton.DrawButtonText("<",
                                                      () =>
                                                      {
                                                          itemTransferAmount -= 1 * modifier;
                                                          SoundDefOf.Tick_High.PlayOneShotOnCamera();
                                                      },
                                                      itemTransferAmount == -amountInStorage);

                itemIncreaseAmountButton.DrawButtonText(">",
                                                        () =>
                                                        {
                                                            Log.Message($"width: {TradeArrow.width}, height: {TradeArrow.height})");
                                                            itemTransferAmount += 1 * modifier;
                                                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                                                        },
                                                        itemTransferAmount == amountOnMap);

                if (itemTransferAmount != 0)
                {
                    if (amountIs0OrMore)
                    {
                        GUI.DrawTexture(itemNumericFieldTexture, TradeArrow);
                    }
                    else if (amountIs0OrLess) GUI.DrawTexture(itemNumericFieldTexture.FlipHorizontal(), TradeArrow);
                }

                //TooltipHandler.TipRegion(itemRectTransferAmount, "Empire_ITW_AmountOfItemsTransferred".Translate());

                Text.Anchor = TextAnchor.MiddleRight;
                MouseoverSounds.DoRegion(itemRectStorageAmount);
                Widgets.DrawHighlightIfMouseover(itemRectStorageAmount);
                Widgets.Label(itemRectStorageAmount, amountInStorage.ToString());
                TooltipHandler.TipRegion(itemRectStorageAmount, "Empire_ITW_AmountStoredInStorage".Translate());

                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.InfoCardButton(itemInfoRect, thing);

                //int moveAmount = Mathf.Clamp(itemTransferAmount, -amountInStorage, amountOnMap);

                transferAmounts[thing] = itemTransferAmount;
                transferBuffer[thing] = itemTransferAmount.ToString();
                count++;
            }

            Widgets.EndScrollView();
            Widgets.DrawBox(rectItemTransferOuter);
        }

        private void DrawTransferTopPart()
        {
            Widgets.DrawLightHighlight(rectItemTransferTopBottom);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLabelDesc.MoveRect(new Vector2(5f, 0f)), "Empire_ITW_SelectItemsToTransfer".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DisplayStoredItems()
        {
            Widgets.DrawLightHighlight(rectStorageManagerListTop.BottomPartPixels(28f));
            Widgets.DrawBox(rectStorageManagerListOuter);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectStorageManagerListTop.MoveRect(new Vector2(5f, 0f)).BottomPartPixels(28f), "Empire_ITW_StoredItems".Translate());
            Widgets.BeginScrollView(rectStorageManagerListOuter, ref storageManagerScroll, rectStorageManagerListInner);

            int count = 0;
            foreach ((ThingDef thing, int storedAmount) in playerEmpire.StorageTracker.StoredThings)
            {
                Rect itemRect = rectStorageItem.MoveRect(new Vector2(0f, rectStorageItem.height * count));
                Rect itemRectIcon = itemRect.LeftPartPixels(itemRect.height).ContractedBy(4f);
                Rect itemRectLabel = itemRect.MoveRect(new Vector2(itemRectIcon.width + 5f, 0f));

                itemRect.DoRectHighlight(count % 2 == 1);
                Widgets.ThingIcon(itemRectIcon, thing);
                Widgets.Label(itemRectLabel, $"{thing?.LabelCap} {storedAmount} ({(Rand.Value > 0.5 ? "+" : "-")} {Rand.Range(0, 100)})");
                Widgets.InfoCardButton(itemRect.RightPartPixels(itemRect.height).ContractedBy(4f), thing);

                count++;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndScrollView();
        }

        private void DrawTop()
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(rectTop, "Empire_ITW_Title".Translate());
            Text.Font = GameFont.Small;

            Widgets.DrawLineHorizontal(rectTop.x, rectTop.yMax, rectTop.width);
        }

        private void DrawBottom()
        {
            Widgets.DrawLineHorizontal(rectBot.x, rectBot.y, rectBot.width);
            rectButtonApply.DrawButtonText("Empire_ITW_Apply".Translate(), ApplyAction);
            GUI.color = Color.white;
        }

        private void ApplyAction()
        {
            throw new NotImplementedException();
        }
    }
}
