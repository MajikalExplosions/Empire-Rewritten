using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace Empire_Rewritten.Emblems
{
    public class Emblem
    {
        private Color iconColor, backColor, accentColor;
        private EmblemPartDef iconPart, patternPart, shapePart;
        
        public Color IconColor
        {
            get => iconColor;
            set
            {
                if (iconColor != null && iconColor == value) return;
                ChangedSinceGeneration = true;
                iconColor = value;
            }
        }
        
        public Color BackColor
        {
            get { return backColor; }
            set
            {
                if (backColor != null && backColor == value) return;
                ChangedSinceGeneration = true;
                backColor = value;
            }
        }

        public Color AccentColor
        {
            get { return accentColor; }
            set
            {
                if (accentColor != null && accentColor == value) return;
                ChangedSinceGeneration = true;
                accentColor = value;
            }
        }
        
        public EmblemPartDef Icon
        {
            get { return iconPart; }
            set
            {
                if (iconPart != null && iconPart == value) return;
                ChangedSinceGeneration = true;
                iconPart = value;
            }
        }

        public EmblemPartDef Pattern
        {
            get { return patternPart; }
            set
            {
                if (patternPart != null && patternPart == value) return;
                ChangedSinceGeneration = true;
                patternPart = value;
            }
        }

        public EmblemPartDef Shape
        {
            get { return shapePart; }
            set
            {
                if (shapePart != null && shapePart == value) return;
                ChangedSinceGeneration = true;
                shapePart = value;
            }
        }

        private bool ChangedSinceGeneration;
        private Texture2D Texture;

        public Emblem(Color iconColor, Color backColor, Color accentColor, EmblemPartDef icon, EmblemPartDef pattern, EmblemPartDef shape)
        {
            ChangedSinceGeneration = true;
            IconColor = iconColor;
            BackColor = backColor;
            AccentColor = accentColor;
            Icon = icon;
            Pattern = pattern;
            Shape = shape;
        }

        public Emblem() : this(PresetColors[0, 0], PresetColors[0, 1], PresetColors[1, 0],
            DefDatabase<EmblemPartDef>.GetNamed("icon_empty"), DefDatabase<EmblemPartDef>.GetNamed("pattern_empty"), DefDatabase<EmblemPartDef>.GetNamed("shape_empty")) { }

        public Texture2D GetTexture()
        {
            if (ChangedSinceGeneration || Texture == null)
            {
                Texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT);
                if (! VerifyTextures())
                {
                    Utils.Logger.Error("Selected textures have different sizes!");
                    return Texture;
                }

                Color[] icon = Icon.Texture.GetPixels();
                Color[] pattern = Pattern.Texture.GetPixels();
                Color[] shape = Shape.Texture.GetPixels();

                if (icon.Length != pattern.Length || icon.Length != shape.Length || pattern.Length != shape.Length)
                {
                    Utils.Logger.Error("Selected textures have different sizes!");
                    return Texture;
                }

                Color[] output = new Color[icon.Length];

                for (int i = 0; i < output.Length; i++) {
                    //Start with colored shape
                    output[i] = shape[i] * BackColor;

                    //Add together output/pattern, with pattern taking more the higher its albedo. Mask result with respect to shape.
                    if (pattern[i].a != 0) output[i] = ScaleAlbedo(output[i], pattern[i] * AccentColor, mask: true);

                    //Add icon on top, overwriting whatever's below it.
                    if (icon[i].a != 0) output[i] = ScaleAlbedo(output[i], icon[i] * IconColor);
                }
                Texture.SetPixels(output);
                Texture.Apply(true, true);

            }
            ChangedSinceGeneration = false;
            return Texture;
        }

        private static Dictionary<EmblemPartCategory, List<EmblemPartDef>> _partCache = new Dictionary<EmblemPartCategory, List<EmblemPartDef>>();

        public static List<EmblemPartDef> GetPartsFor(EmblemPartCategory category)
        {
            if (_partCache.ContainsKey(category)) return _partCache[category];
            List<EmblemPartDef> validParts = new List<EmblemPartDef>();
            foreach (EmblemPartDef part in DefDatabase<EmblemPartDef>.AllDefs)
            {
                if (part.category == category) validParts.Add(part);
            }
            _partCache[category] = validParts;
            return _partCache[category];
        }
        private Color ScaleAlbedo(Color bot, Color top, bool mask = false)
        {
            Color c = bot * (1 - top.a) + top * top.a;
            if (mask) c.a *= bot.a;
            return c;
        }
        private bool VerifyTextures()
        {
            if (Icon.Texture.width != TEXTURE_WIDTH || Icon.Texture.height != TEXTURE_HEIGHT ||
                Pattern.Texture.width != TEXTURE_WIDTH || Pattern.Texture.height != TEXTURE_HEIGHT ||
                Shape.Texture.width != TEXTURE_WIDTH || Shape.Texture.height != TEXTURE_HEIGHT) return false;
            return true;
        }

        /*
         * First 3 rows mostly from Stellaris (flags/colors.txt, if you own the game. The "flag" colors, not "ship"):
         * intense_red, orange, yellow, green
         * intense_turquoise, intense_blue, indigo, intense_purple
         * intense_pink, beige, dark_brown, <Rimworld UI Button Color>
         * 
         * Last row is:
         * off-white, light gray, dark gray, off-black
         */
        public static readonly Color[,] PresetColors = {
            { FromInt(241, 37, 15), FromInt(215, 100, 35), FromInt(204, 148, 41), FromInt(46, 102, 41) },
            { FromInt(55, 178, 170), FromInt(30, 159, 220), FromInt(47, 19, 127), FromInt(139, 39, 184) },
            { FromInt(176, 49, 128), FromInt(150, 126, 90), FromInt(58, 38, 23), FromInt(105, 81, 45) },
            { FromInt(239, 239, 239), FromInt(153, 153, 153), FromInt(51, 51, 51), FromInt(16, 16, 16) }
        };

        private static Color FromInt(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public const int TEXTURE_WIDTH = 512, TEXTURE_HEIGHT = 512;
    }
}
