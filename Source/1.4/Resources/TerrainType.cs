using Verse;

namespace Empire_Rewritten.Resources
{
    public enum TerrainType
    {
        Ocean = 0,
        Lake = 1,
        Flat = 2,
        SmallHills = 3,
        LargeHills = 4,
        Mountainous = 5,
        Impassable = 6
    }

    public enum RiverType
    {
        None = 0,
        Creek = 1,
        River = 2,
        LargeRiver = 3,
        HugeRiver = 4
    }

    public static class TerrainTypeExtensions
    {
        public static string Translate(this TerrainType stat, bool isOffset)
        {
            return $"Empire_ResourceInfoWindow{stat.ToString().CapitalizeFirst()}".TranslateSimple();
        }

        public static bool IsWaterBody(this TerrainType stat)
        {
            return stat < TerrainType.Flat;
        }

        public static bool IsBuildable(this TerrainType stat)
        {
            return stat >= TerrainType.Flat && stat <= TerrainType.LargeHills;
        }
    }
}