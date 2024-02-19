using RimWorld;
using System;
using Verse;

namespace Empire_Rewritten.Resources
{
    public class BiomeModifier : ResourceModifier
    {
        public BiomeDef biome;

        public BiomeModifier(ResourceDef resourceDef) : base(resourceDef) { }
    }

    public class ResourceModifier : IExposable
    {
        public ResourceDef def;
        public float offset = 0f;
        public float multiplier = 1f;

        public ResourceModifier() { }

        public ResourceModifier(ResourceDef resourceDef, float offsetValue = 0, float multiplierValue = 1)
        {
            def = resourceDef;
            offset = offsetValue;
            multiplier = multiplierValue;
        }

        public ResourceModifier MergeWith(ResourceModifier other)
        {
            if (other.def != def) throw new ArgumentOutOfRangeException(nameof(other), "Merging modifiers with different ResourceDef types");
            return new ResourceModifier(def, other.offset + offset, other.multiplier * multiplier);
        }

        public void MergeInto(ResourceModifier other)
        {
            if (other.def != def) throw new ArgumentOutOfRangeException(nameof(other), "Merging modifiers with different ResourceDef types");
            offset += other.offset;
            multiplier *= other.multiplier;
        }

        public float TotalModifier()
        {
            return (1 + offset) * multiplier;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref offset, "offset");
            Scribe_Values.Look(ref multiplier, "multiplier");
        }
    }
}