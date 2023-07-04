using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Empire_Rewritten.Utils;

namespace Empire_Rewritten.Utils
{
    /// <summary>
    ///     This class provides some useful and often used basic window operations
    /// </summary>
    [StaticConstructorOnStartup]
    public static class WindowHelper
    {
        private static Texture2D corner;
        private static Texture2D cornerLight;

        private static Texture2D CornerHighlight => corner ?? (corner = CreateCornerHighlight(new Color(1f, 1f, 1f, 0.1f)));
        private static Texture2D CornerLightHighlight => cornerLight ?? (cornerLight = CreateCornerHighlight(new Color(1f, 1f, 1f, 0.04f)));

        public static Texture2D CreateCornerHighlight(Color highlightColor)
        {
            if (!UnityData.IsInMainThread)
            {
                Log.Error("Tried to create a texture from a different thread.");
                return null;
            }

            Texture2D texture2D = new Texture2D(5, 5)
            {
                name = "CornerTexture"
            };

            float baseAlpha = highlightColor.a;

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (x == y)
                    {
                        highlightColor.a = baseAlpha * 0.6f;
                    }

                    if (x + 1 == y)
                    {
                        highlightColor.a = baseAlpha * 0.125f;
                    }

                    if (x > y)
                    {
                        highlightColor.a = 0f;
                    }

                    if (x < y)
                    {
                        highlightColor.a = baseAlpha;
                    }

                    texture2D.SetPixel(x, 4 - y, highlightColor);
                }
            }
            
            texture2D.Apply();
            return texture2D;
        }

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
        public static void DrawBorderAroundRect(this Rect rect, int width = 1, Color? maybeColor = null)
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

        /// <summary>
        ///     Creates a <see cref="Widgets.ButtonInvisible(Rect, bool)" /> using a given <see cref="string" /> as label, inside a
        ///     given <see cref="Rect" />
        /// </summary>
        /// <param name="inRect">The <see cref="Rect" /> to draw in</param>
        /// <param name="label">The <see cref="string" /> that should be used as label</param>
        /// <returns><c>true</c> if the button is pressed, <c>false</c> otherwise</returns>
        public static bool DrawInfoScreenSelectorButton(Rect inRect, string label)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;

            Widgets.DrawLightHighlight(inRect);
            Widgets.DrawHighlightIfMouseover(inRect);
            Widgets.Label(inRect, label);
            Widgets.DrawBox(inRect, 2);

            ResetTextAndColor();

            return Widgets.ButtonInvisible(inRect);
        }

        /// <summary>
        ///     Draws boxes around all the <paramref name="rects"/> given.
        /// </summary>
        /// <param name="rects"></param>
        public static void DrawBoxes(Rect[] rects)
        {
            for (int i = 0; i < rects.Length; i++)
            {
                Widgets.DrawBox(rects[i]);
            }
        }

        /// <summary>
        ///     Draws boxes around all the <paramref name="rects"/> given.
        /// </summary>
        /// <param name="rects"></param>
        public static void DrawBoxes(IEnumerable<Rect> rects)
        {
            foreach (Rect rect in rects)
            {
                Widgets.DrawBox(rect);
            }
        }

        public static bool InfoCardButtonWorker(Rect rect)
        {
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
            bool result = Widgets.ButtonImage(rect, TexButton.Info, GUI.color, true);
            UIHighlighter.HighlightOpportunity(rect, "InfoCard");
            return result;
        }

        public static void DrawCorneredHighlight(Rect rect)
        {
            Rect mainRect = new Rect(rect.x, rect.y, rect.width - 5f, rect.height - 5f);
            Rect bottomEdge = new Rect(rect.x + 5f, rect.yMax - 5f, rect.width - 10f, 5f);
            Rect rightEdge = new Rect(rect.xMax - 5f, rect.y + 5f, 5f, rect.height - 5f);

            Rect topRightCorner = new Rect(rect.xMax - 5f, rect.y, 5f, 5f);
            Rect bottomLeftCorner = new Rect(rect.x + 5f, rect.yMax, -5f, -5f);

            Widgets.DrawHighlight(mainRect);
            Widgets.DrawHighlight(bottomEdge);
            Widgets.DrawHighlight(rightEdge);

            GUI.DrawTexture(topRightCorner, CornerHighlight);
            GUI.DrawTexture(bottomLeftCorner, CornerHighlight);
        }

        public static void DrawLightCorneredHighlight(Rect rect)
        {
            Rect mainRect = new Rect(rect.x, rect.y, rect.width - 5f, rect.height - 5f);
            Rect bottomEdge = new Rect(rect.x + 5f, rect.yMax - 5f, rect.width - 10f, 5f);
            Rect rightEdge = new Rect(rect.xMax - 5f, rect.y + 5f, 5f, rect.height - 5f);

            Rect topRightCorner = new Rect(rect.xMax - 5f, rect.y, 5f, 5f);
            Rect bottomLeftCorner = new Rect(rect.x + 5f, rect.yMax, -5f, -5f);

            Widgets.DrawLightHighlight(mainRect);
            Widgets.DrawLightHighlight(bottomEdge);
            Widgets.DrawLightHighlight(rightEdge);

            GUI.DrawTexture(topRightCorner, CornerLightHighlight);
            GUI.DrawTexture(bottomLeftCorner, CornerLightHighlight);
        }

        /// <summary>
        /// Draws a confirmation dialog at a given location with two labels. If either one is clicked, the corresponding
        /// <see cref="Action"/> is performed and the function returns true. Otherwise, it returns false.
        /// </summary>
        /// <param name="rect">The location to draw the dialog.</param>
        /// <param name="Question">The prompt for the dialog.</param>
        /// <param name="OptionLabel0">The text in the first button</param>
        /// <param name="OptionLabel1">The text in the second button</param>
        /// <param name="OptionAction0">The action corresponding to pressing the first button</param>
        /// <param name="OptionAction1">THe aciton corresponding to pressing the second button</param>
        /// <returns>True if either button is pressed; false otherwise</returns>
        public static bool ConfirmDialog(Rect rect, String Question, String OptionLabel0, String OptionLabel1, Action OptionAction0, Action OptionAction1)
        {
            FlexRect root = new FlexRect("root");
            root.Top(0.9f, "main");
            root.BottomLeft(0.5f, 0.125f, "option0");
            root.BottomRight(0.5f, 0.125f, "option1");
            Dictionary<string, Rect> rects = root.Resolve(rect);


            ResetTextAndColor();
            Widgets.DrawBox(rects["root"]);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rects["main"], Question);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Widgets.ButtonText(rects["option0"].ContractedBy(5f), OptionLabel0))
            {
                OptionAction0.Invoke();
                return true;
            }
            if (Widgets.ButtonText(rects["option1"].ContractedBy(5f), OptionLabel1))
            {
                OptionAction1.Invoke();
                return true;
            }
            return false;
        }
    }
}
