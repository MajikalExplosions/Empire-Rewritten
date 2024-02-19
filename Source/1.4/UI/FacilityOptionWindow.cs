using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.UI
{
    public class FacilityOptionWindow : Window
    {
        private FlexRect _root;
        private Facility _facility;

        public override Vector2 InitialSize => new Vector2(300, 300);

        public FacilityOptionWindow(Facility facility)
        {
            closeOnClickedOutside = true;
            doCloseX = false;
            onlyOneOfTypeAllowed = true;
            draggable = true;

            _facility = facility;

            _root = new FlexRect("root");
            _root.Top(0.333f, "info");
            FlexRect bot = _root.Bottom(0.667f, "options");
            bot.Grid(1, 4, 0, 0, "button0");
            bot.Grid(1, 4, 0, 1, "button1");
            bot.Grid(1, 4, 0, 2, "button2");
            bot.Grid(1, 4, 0, 3, "button3");

            _root.MergeChildren();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Dictionary<string, Rect> rects = _root.Resolve(inRect);
            WindowHelper.DrawFacilityBox(rects["info"], _facility);

            // Draw buttons
            string reason;
            bool canExpand = _facility.Settlement.GetDetails().CanUpgrade(_facility.def, out reason);
            bool canDownsize = _facility.Size > 1;
            bool canDemolish = _facility.Size > 0;

            if (canExpand && Widgets.ButtonText(rects["button0"], "Empire_FOW_ButtonExpand".Translate()))
            {
                _facility.Settlement.GetDetails().UpgradeFacility(_facility.def);
            }
            else if (!canExpand)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["button0"], reason.Translate());

                if (reason == "Empire_SD_ReasonTooExpensive")
                {
                    TooltipHandler.TipRegion(rects["button0"], _facility.def.GetCostString());
                }
            }

            if (canDownsize && Widgets.ButtonText(rects["button1"], "Empire_FOW_ButtonDownsize".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Empire_FOW_ConfirmDownsize".Translate(), delegate
                {
                    _facility.ChangeSize(-1);
                }));
            }
            else if (!canDownsize)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["button1"], "Empire_FOW_ReasonDownsize".Translate());
            }

            if (canDemolish && Widgets.ButtonText(rects["button2"], "Empire_FOW_ButtonDemolish".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Empire_FOW_ConfirmDemolish".Translate(), delegate
                {
                    _facility.Settlement.GetDetails().DemolishFacility(_facility.def);
                    Close();
                }));
            }
            else if (!canDemolish)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rects["button2"], "Empire_FOW_ReasonDemolish".Translate());
            }

            if (Widgets.ButtonText(rects["button3"], "Cancel"))
            {
                Close();
            }

            WindowHelper.ResetTextAndColor();
        }

    }
}
