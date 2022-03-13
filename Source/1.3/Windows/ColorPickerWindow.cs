﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Empire_Rewritten.Utils;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Windows
{
    public class ColorPickerWindow : Window
    {
        private const int CreatedBoxes = 4;
        private const int ColorComponentHeight = 200;
        private const int HueBarWidth = 20;

        private readonly string[] colorBuffers = {"255", "255", "255"};

        private readonly Regex hexRx = new Regex(@"[^a-f0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Texture2D hueBarTexture = new Texture2D(1, ColorComponentHeight);
        private readonly Rect rectColorInput;
        private readonly List<Rect> rectColorInputBoxes;
        private readonly Rect rectFull = new Rect(0f, 0f, 600f, 600f);

        private readonly Rect rectHueBar;
        private readonly Rect rectMain;
        private readonly List<Rect> rectRGBInputBoxes;

        private readonly int[] rectRGBValues = {0, 0, 0};
        private readonly Rect rectSaturationValueSquare;

        private bool hexChanged = true;
        private string hexCode = "#FFFFFF";
        private bool rgbChanged;

        private Color selectedColor = Color.red;

        public ColorPickerWindow()
        {
            rectMain = new Rect(rectFull).ContractedBy(25f);
            rectSaturationValueSquare = new Rect(rectMain.position, new Vector2(ColorComponentHeight, ColorComponentHeight));
            rectHueBar = rectSaturationValueSquare.MoveRect(new Vector2(rectSaturationValueSquare.width + 10f, 0f)).LeftPartPixels(HueBarWidth);
            rectColorInput = rectHueBar.MoveRect(new Vector2(rectHueBar.width + 10f, 0f));
            rectColorInput.size = new Vector2(rectMain.width - rectColorInput.position.x + 25f, rectSaturationValueSquare.height);
            rectColorInputBoxes = rectColorInput.DivideVertical(CreatedBoxes).ToList();
            rectRGBInputBoxes = rectColorInputBoxes[3].DivideHorizontal(3).ToList();

            for (int y = 0; y < ColorComponentHeight; y++)
            {
                hueBarTexture.SetPixel(0, y, Color.HSVToRGB((float)y / ColorComponentHeight, 1, 1));
            }

            hueBarTexture.Apply();
        }

        private Color SelectedColor
        {
            get => selectedColor;
            set
            {
                selectedColor = value;
                UpdateColor();
            }
        }

        public override Vector2 InitialSize => rectFull.size;

        protected override float Margin => 0f;

        private void UpdateColor()
        {
            // TODO: Use this function to set all of the widgets' values if one of them changes SelectedColor
            //       e.g. changing the red value text widget should update the hue of the SV Square
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawCloseButton(inRect);

            DrawSaturationValueSquare();
            DrawHueBar();

            WindowHelper.DrawBoxes(new[] {rectMain, rectSaturationValueSquare, rectHueBar, rectColorInput});

            DrawInputFieldLabels();
            DrawHexCodeInputField();
            DrawRGBInputValues();
        }

        private void DrawHueBar()
        {
            GUI.DrawTexture(rectHueBar, hueBarTexture);
        }

        private void DrawSaturationValueSquare()
        {
            Texture2D texture = new Texture2D(ColorComponentHeight, ColorComponentHeight)
            {
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave // TODO: Check out what these two things actually are
            };

            Color[] colors = new Color[ColorComponentHeight * ColorComponentHeight];
            for (int x = 0; x < ColorComponentHeight; x++)
            {
                for (int y = 0; y < ColorComponentHeight; y++)
                {
                    colors[x + y * ColorComponentHeight] = Color.HSVToRGB(0f, (float)x / ColorComponentHeight, (float)y / ColorComponentHeight);
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            GUI.DrawTexture(rectSaturationValueSquare, texture);
            if (Widgets.ButtonInvisible(rectSaturationValueSquare))
            {
                Vector2 mousePositionInRect = Event.current.mousePosition - rectSaturationValueSquare.position;
                SelectedColor = Color.HSVToRGB(0f, mousePositionInRect.x / ColorComponentHeight, 1f - mousePositionInRect.y / ColorComponentHeight);
            }
        }

        /// <summary>
        ///     Draws the input field labels
        /// </summary>
        private void DrawInputFieldLabels()
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(rectColorInputBoxes[0], "Hex");
            Widgets.Label(rectColorInputBoxes[2], "RGB");
            WindowHelper.ResetTextAndColor();
        }

        /// <summary>
        ///     Creates the <see cref="hexCode" /> value input field
        ///     Changes the <see cref="rectRGBValues" /> when a new value is inputted
        /// </summary>
        private void DrawHexCodeInputField()
        {
            if (hexRx.IsMatch(hexCode.Substring(1)) || hexCode.Length != 7) //Mark the field red if there is an error
            {
                GUI.color = Color.red;
            }
            else if (hexChanged) //Only changes if the hexcode is legal
            {
                for (int i = 0; i < 3; i++)
                {
                    rectRGBValues[i] = int.Parse(hexCode.Substring(1 + 2 * i, 2), NumberStyles.HexNumber);
                }

                hexChanged = false;
            }

            string hexBefore = hexCode;
            hexCode = Widgets.TextField(rectColorInputBoxes[1].ContractedBy(5f), hexCode);
            hexChanged = !hexBefore.Equals(hexCode) || hexChanged;
            GUI.color = Color.white;

            //Checks if a hex code starts with the # char and sets it if it's missing
            if (!hexCode.StartsWith("#"))
            {
                hexCode = $"#{hexCode}";

                //Fixes the # char being moved to the third position if someone writes before it
                if (hexCode.Length >= 3 && hexCode[2].Equals('#'))
                {
                    hexCode = $"{hexCode.Substring(0, 2)}{(hexCode.Length >= 4 ? hexCode.Substring(4) : string.Empty)}";
                }
            }
        }

        /// <summary>
        ///     Creates the RGB value input fields and stores the inputs inside <see cref="rectRGBValues" />
        ///     Changes the <see cref="hexCode" /> when new values are inputted
        /// </summary>
        private void DrawRGBInputValues()
        {
            //Creates the RGB value inputs and handles them
            for (int i = 0; i < 3; i++)
            {
                int value = rectRGBValues[i];
                string colorBuffer = colorBuffers[i];

                int colorBefore = value;
                Widgets.TextFieldNumeric(rectRGBInputBoxes[i].ContractedBy(5f), ref value, ref colorBuffer, 0f, 255f);

                rectRGBValues[i] = value;
                colorBuffers[i] = colorBuffer;

                if (!colorBefore.Equals(rectRGBValues[i]))
                {
                    rgbChanged = true;
                }
            }

            //Adjusts the hexCode if the rgb values were changed
            if (rgbChanged)
            {
                hexCode = "#";

                for (int i = 0; i < 3; i++)
                {
                    hexCode += rectRGBValues[i].ToString("X2");
                }

                rgbChanged = false;
            }
        }

        private void DrawCloseButton(Rect inRect)
        {
            if (Widgets.CloseButtonFor(inRect)) Close();
        }
    }
}
