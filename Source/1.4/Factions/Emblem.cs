using Empire_Rewritten.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Logger = Empire_Rewritten.Utils.Logger;

namespace Empire_Rewritten.Factions
{
    public class Emblem : IExposable
    {
        public const int TEXTURE_SIZE = 128, TEXTURE_SMALL_SIZE = 32;

        private Color iconColor, backColor, accentColor;
        private EmblemPartDef iconPart, patternPart, shapePart;

        public Color IconColor
        {
            get => iconColor;
            set
            {
                if (iconColor != null && iconColor == value) return;
                _changedSinceGeneration = true;
                iconColor = value;
            }
        }

        public Color BackColor
        {
            get { return backColor; }
            set
            {
                if (backColor != null && backColor == value) return;
                _changedSinceGeneration = true;
                backColor = value;
            }
        }

        public Color AccentColor
        {
            get { return accentColor; }
            set
            {
                if (accentColor != null && accentColor == value) return;
                _changedSinceGeneration = true;
                accentColor = value;
            }
        }

        public EmblemPartDef Icon
        {
            get { return iconPart; }
            set
            {
                if (iconPart != null && iconPart == value) return;
                _changedSinceGeneration = true;
                iconPart = value;
            }
        }

        public EmblemPartDef Pattern
        {
            get { return patternPart; }
            set
            {
                if (patternPart != null && patternPart == value) return;
                _changedSinceGeneration = true;
                patternPart = value;
            }
        }

        public EmblemPartDef Shape
        {
            get { return shapePart; }
            set
            {
                if (shapePart != null && shapePart == value) return;
                _changedSinceGeneration = true;
                shapePart = value;
            }
        }

        private bool _changedSinceGeneration;
        private Texture2D _texture, _textureSmall;

        public Emblem(Color iconColor, Color backColor, Color accentColor, EmblemPartDef icon, EmblemPartDef pattern, EmblemPartDef shape)
        {
            _changedSinceGeneration = true;
            IconColor = iconColor;
            BackColor = backColor;
            AccentColor = accentColor;
            Icon = icon;
            Pattern = pattern;
            Shape = shape;
        }

        public Emblem() : this(PresetColors[0, 0], PresetColors[0, 1], PresetColors[1, 0],
            DefDatabase<EmblemPartDef>.GetNamed("icon_empty"), DefDatabase<EmblemPartDef>.GetNamed("pattern_empty"), DefDatabase<EmblemPartDef>.GetNamed("shape_empty"))
        { }

        public Texture2D GetTexture()
        {
            if (_changedSinceGeneration || _texture == null)
            {
                _texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE);
                if (!_VerifyTextures())
                {
                    return _texture;
                }

                Logger.Log($"Emblem Details: {Icon.label}, {Pattern.label}, {Shape.label}");
                Color[] icon = Icon.Texture.GetPixels();
                Color[] pattern = Pattern.Texture.GetPixels();
                Color[] shape = Shape.Texture.GetPixels();

                Color[] output = new Color[icon.Length];

                for (int i = 0; i < output.Length; i++)
                {
                    //Start with colored shape
                    output[i] = shape[i] * BackColor;

                    //Add together output/pattern, with pattern taking more the higher its albedo. Mask result with respect to shape.
                    if (pattern[i].a != 0) output[i] = _ScaleAlbedo(output[i], pattern[i] * AccentColor, mask: true);

                    //Add icon on top, overwriting whatever's below it.
                    if (icon[i].a != 0) output[i] = _ScaleAlbedo(output[i], icon[i] * IconColor);
                }
                _texture.SetPixels(output);
                _texture.Apply(true);

                // Resize copy to 32x32 (not crop)
                _textureSmall = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE);
                _textureSmall.SetPixels(output);
                TextureUtils.Scale(ref _textureSmall, TEXTURE_SMALL_SIZE, TEXTURE_SMALL_SIZE);
                /*
                if (! _textureSmall.Resize(TEXTURE_SMALL_SIZE, TEXTURE_SMALL_SIZE))
                {
                    Logger.Warn("Failed to create small emblem texture.");
                }
                */
                _textureSmall.Apply(true);

            }
            _changedSinceGeneration = false;
            return _texture;
        }

        private static Dictionary<EmblemPartCategory, List<EmblemPartDef>> _partCache = new Dictionary<EmblemPartCategory, List<EmblemPartDef>>();
        public static List<EmblemPartDef> GetPartsFor(EmblemPartCategory category)
        {
            if (_partCache.ContainsKey(category)) return _partCache[category];
            List<EmblemPartDef> validParts = new List<EmblemPartDef>();
            foreach (EmblemPartDef part in DefDatabase<EmblemPartDef>.AllDefs)
            {
                if (part.category == category && part.label != "Empty") validParts.Add(part);
            }
            _partCache[category] = validParts;
            return _partCache[category];
        }
        private Color _ScaleAlbedo(Color bot, Color top, bool mask = false)
        {
            Color c = bot * (1 - top.a) + top * top.a;
            if (mask) c.a *= bot.a;
            return c;
        }
        private bool _VerifyTextures()
        {
            if (Icon.Texture.width != TEXTURE_SIZE || Icon.Texture.height != TEXTURE_SIZE ||
                Pattern.Texture.width != TEXTURE_SIZE || Pattern.Texture.height != TEXTURE_SIZE ||
                Shape.Texture.width != TEXTURE_SIZE || Shape.Texture.height != TEXTURE_SIZE) return false;
            return true;
        }

        public static Emblem GetRandomEmblem()
        {
            Emblem emblem = new Emblem();
            emblem.Icon = GetPartsFor(EmblemPartCategory.Icon).RandomElement();
            emblem.Pattern = GetPartsFor(EmblemPartCategory.Pattern).RandomElement();
            emblem.Shape = GetPartsFor(EmblemPartCategory.Shape).RandomElement();
            emblem.IconColor = _PresetColorsAsList.RandomElement();
            emblem.BackColor = _PresetColorsAsList.RandomElement();
            emblem.AccentColor = _PresetColorsAsList.RandomElement();
            emblem._changedSinceGeneration = true;

            return emblem;
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
            { _FromInt(241, 37, 15), _FromInt(215, 100, 35), _FromInt(204, 148, 41), _FromInt(46, 102, 41) },
            { _FromInt(55, 178, 170), _FromInt(30, 159, 220), _FromInt(47, 19, 127), _FromInt(139, 39, 184) },
            { _FromInt(176, 49, 128), _FromInt(150, 126, 90), _FromInt(58, 38, 23), _FromInt(105, 81, 45) },
            { _FromInt(239, 239, 239), _FromInt(153, 153, 153), _FromInt(51, 51, 51), _FromInt(16, 16, 16) }
        };

        private static List<Color> _PresetColorsAsList => PresetColors.Cast<Color>().ToList();

        private static Color _FromInt(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref iconColor, "iconColor");
            Scribe_Values.Look(ref backColor, "backColor");
            Scribe_Values.Look(ref accentColor, "accentColor");

            Scribe_Defs.Look(ref iconPart, "iconPart");
            Scribe_Defs.Look(ref patternPart, "patternPart");
            Scribe_Defs.Look(ref shapePart, "shapePart");
        }
    }
}
