using Empire_Rewritten.Settlements;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Empire_Rewritten.UI
{
    /// <summary>
    ///     This class provides some useful and often used basic window operations
    /// </summary>
    [StaticConstructorOnStartup]
    public static class WindowHelper
    {

        /// <summary>
        ///     This draws a border around a <see cref="Rect" />, using a given <see cref="int">border width</see> and
        ///     <see cref="Color" />
        /// </summary>
        /// <param name="rect">The <see cref="Rect" /> to draw a border around</param>
        /// <param name="width">The border width</param>
        /// <param name="maybeColor">
        ///     The <see cref="Color">Color?</see> of the border to draw. If null, Defaults to
        ///     <see cref="Color.white">white</see>
        /// </param>
        public static void DrawBorderAroundRect(Rect rect, int width = 1, Color? maybeColor = null)
        {
            Rect borderRect = rect.ExpandedBy(width);
            GUI.color = maybeColor ?? Color.white;
            Widgets.DrawBox(borderRect, width);
            GUI.color = Color.white;
        }

        /// <summary>
        ///     Resets the Text.Font, Text.Anchor and GUI.color setting
        /// </summary>
        public static void ResetTextAndColor()
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        public static void DrawFacilityBox(Rect rect, Facility facility)
        {
            ResetTextAndColor();

            // Outline
            Widgets.DrawBox(rect, 2);
            rect = rect.ContractedBy(4f);

            // Divide the box into two parts
            Rect bottom = rect.BottomPartPixels(16);
            rect = rect.TopPartPixels(rect.height - 16);

            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height, rect.width);
            ResetTextAndColor();

            // Draw icon in square on the left
            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            GUI.DrawTexture(iconRect, facility.def.IconTexture, ScaleMode.ScaleToFit);

            // Draw name and size on the right
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect, facility.def.LabelCap);
            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(rect, "Size " + facility.Size.ToString());

            // Draw build progress on the bottom
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            float progress = facility.Settlement.GetDetails().UpgradeProgress(facility.def);
            if (progress >= 0f)
            {
                Widgets.FillableBar(bottom.ContractedBy(1f), progress);
                Widgets.Label(bottom, progress.ToStringPercent());
            }
            else Widgets.Label(bottom, "Nothing in progress");

            ResetTextAndColor();
        }
    }
}
