namespace Empire_Rewritten.Resources
{
    public class TerrainFactors
    {
        public float ocean;
        public float lake;
        public float flat;
        public float smallHills;
        public float largeHills;
        public float mountainous;
        public float impassable;

        public TerrainFactors() { }

        public float GetFactor(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Ocean:
                    return ocean;
                case TerrainType.Lake:
                    return lake;
                case TerrainType.Flat:
                    return flat;
                case TerrainType.SmallHills:
                    return smallHills;
                case TerrainType.LargeHills:
                    return largeHills;
                case TerrainType.Mountainous:
                    return mountainous;
                case TerrainType.Impassable:
                    return impassable;
                default:
                    return 0f;
            }
        }
    }

    public class RiverFactors
    {
        public float none;
        public float creek;
        public float river;
        public float largeRiver;
        public float hugeRiver;

        public RiverFactors() { }

        public float GetFactor(RiverType riverType)
        {
            switch (riverType)
            {
                case RiverType.None:
                    return none;
                case RiverType.Creek:
                    return creek;
                case RiverType.River:
                    return river;
                case RiverType.LargeRiver:
                    return largeRiver;
                case RiverType.HugeRiver:
                    return hugeRiver;
                default:
                    return 0f;
            }
        }
    }
}