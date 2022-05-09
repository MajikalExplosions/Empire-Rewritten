using System;
using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Resources.Stats;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Resources
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class ResourceDef : Def
    {
        [NotNull] public readonly ThingFilter resourcesCreated = new ThingFilter();
        [NotNull] private readonly Dictionary<BiomeDef, ResourceModifier> cachedBiomeModifiers = new Dictionary<BiomeDef, ResourceModifier>();

        [NotNull] public GraphicData iconData;

        [NotNull] public HillinessValues hillinessFactors;
        [NotNull] public HillinessValues hillinessOffsets;

        /// <summary>
        ///     The AI will start trying to get rid of facilities and this resource if it produces more than this number.
        /// </summary>
        public int desiredAIMaximum = 50;

        /// <summary>
        ///     The AI will focus more on this resource if its income is below this amount.
        /// </summary>
        public int desiredAIMinimum = 30;

        [ItemNotNull] public List<BiomeModifier> biomeModifiers;
        [ItemNotNull] public List<StuffCategoryDef> removeStuffCategoryDefs;
        [ItemNotNull] public List<StuffCategoryDef> stuffCategoryDefs;
        [ItemNotNull] public List<ThingCategoryDef> removeThingCategoryDefs;
        [ItemNotNull] public List<ThingCategoryDef> thingCategoryDefs;
        [ItemNotNull] public List<ThingDef> allowedThingDefs;
        [ItemNotNull] public List<ThingDef> postRemoveThingDefs;
        [NotNull] public SimpleCurve heightCurve;
        [NotNull] public SimpleCurve rainfallCurve;
        [NotNull] public SimpleCurve swampinessCurve;
        [NotNull] public SimpleCurve temperatureCurve;
        [CanBeNull] public Type resourceWorker;
        [NotNull] public WaterBodyValues waterBodyFactors;
        [NotNull] public WaterBodyValues waterBodyOffsets;

        private bool hasCachedThingDefs;

        private ResourceWorker worker;

        public Graphic Graphic => iconData.Graphic;

        /// <summary>
        ///     Maintains a cached <see cref="ThingFilter" /> of the <see cref="ThingDef">resources</see> created from this
        ///     <see cref="ResourceDef" />
        /// </summary>
        [NotNull]
        public ThingFilter ResourcesCreated
        {
            get
            {
                if (hasCachedThingDefs) return resourcesCreated;

                resourcesCreated.SetDisallowAll();

                stuffCategoryDefs?.ForEach(category => resourcesCreated.SetAllow(category, true));
                removeStuffCategoryDefs?.ForEach(category => resourcesCreated.SetAllow(category, false));

                thingCategoryDefs?.ForEach(category => resourcesCreated.SetAllow(category, true));
                removeThingCategoryDefs?.ForEach(category => resourcesCreated.SetAllow(category, false));

                allowedThingDefs?.ForEach(thingDef => resourcesCreated.SetAllow(thingDef, true));
                postRemoveThingDefs?.ForEach(thingDef => resourcesCreated.SetAllow(thingDef, false));

                ResourceWorker?.PostModifyThingFilter();

                hasCachedThingDefs = true;

                return resourcesCreated;
            }
        }

        /// <summary>
        ///     Maintains a cached <see cref="ResourceWorker" /> of the <see cref="ResourceDef.resourceWorker">specified Type</see>
        /// </summary>
        public ResourceWorker ResourceWorker =>
            resourceWorker == null ? null : worker ?? (worker = (ResourceWorker)Activator.CreateInstance(resourceWorker, resourcesCreated));

        /// <summary>
        ///     Gets the <see cref="ResourceModifier" /> of a given <see cref="Tile" />
        /// </summary>
        /// <param name="tile">The <see cref="Tile" /> to get the <see cref="ResourceModifier" /> of</param>
        /// <returns>The <see cref="ResourceModifier" /> for <paramref name="tile" /></returns>
        public ResourceModifier GetTileModifier([NotNull] Tile tile)
        {
            ResourceModifier biomeModifier = GetBiomeModifier(tile);

            if (tile.WaterCovered || tile.hilliness == Hilliness.Impassable) return new ResourceModifier(this, biomeModifier.offset, 0f);

            float result = 1;

            float tempVal = temperatureCurve.Evaluate(tile.temperature);
            float heightVal = heightCurve.Evaluate((float)tile.hilliness);
            float swampinessVal = swampinessCurve.Evaluate(tile.swampiness);
            float rainfallVal = rainfallCurve.Evaluate(tile.rainfall);

            float hillFacVal = hillinessFactors.GetValue((ResourceStat)(tile.hilliness - 1));
            float hillOffVal = hillinessOffsets.GetValue((ResourceStat)(tile.hilliness - 1));

            float waterFacVal = waterBodyFactors.GetValueMult(tile);
            float waterOffVal = waterBodyOffsets.GetValueAdd(tile);

            result = result * tempVal * heightVal * biomeModifier.multiplier * swampinessVal * rainfallVal * hillFacVal * waterFacVal +
                     hillOffVal +
                     waterOffVal;

            ResourceModifier modifer = new ResourceModifier(this, biomeModifier.offset, result);

            return modifer;
        }

        /// <summary>
        ///     Checks for the <see cref="float">resource production bonus</see> of a given <see cref="ResourceStat" />.
        ///     Will return offset value if <paramref name="isOffset" /> is true, otherwise the factor value.
        /// </summary>
        /// <param name="stat">The <see cref="ResourceStat" /> to get the bonus of</param>
        /// <param name="isOffset">Whether to get the offset value instead of the factor</param>
        /// <returns>The value of the given <paramref name="stat" /> and <paramref name="isOffset" /> combination</returns>
        public float GetBonus(ResourceStat stat, bool isOffset)
        {
            if (stat.IsWaterBody())
            {
                return isOffset ? waterBodyOffsets.GetValue(stat) : waterBodyFactors.GetValue(stat);
            }

            return isOffset ? hillinessOffsets.GetValue(stat) : hillinessFactors.GetValue(stat);
        }

        /// <summary>
        ///     Gets the <see cref="BiomeDef" />-based <see cref="ResourceModifier" /> of a given <see cref="Tile" />
        /// </summary>
        /// <param name="tile">The <see cref="Tile" /> to get the <see cref="ResourceModifier" /> of</param>
        /// <returns>
        ///     The <see cref="ResourceModifier" /> of the given <see cref="Tile" /> based on its <see cref="Tile.biome" />
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     if <paramref name="tile" />'s <see cref="Tile.biome" /> is <c>null</c>
        /// </exception>
        public ResourceModifier GetBiomeModifier([NotNull] Tile tile)
        {
            BiomeDef biome = tile.biome;
            if (biome is null)
            {
                throw new ArgumentNullException(nameof(tile.biome));
            }

            if (cachedBiomeModifiers.ContainsKey(biome))
            {
                return cachedBiomeModifiers.TryGetValue(biome);
            }

            ResourceModifier modifier = biomeModifiers?.First(x => x.biome == biome) is BiomeModifier biomeModifier
                ? new ResourceModifier(this, biomeModifier.offset, biomeModifier.multiplier)
                : new ResourceModifier(this);

            cachedBiomeModifiers.Add(biome, modifier);
            return modifier;
        }

        public override void ClearCachedData()
        {
            cachedBiomeModifiers.Clear();
            hasCachedThingDefs = false;
            base.ClearCachedData();
        }

        [NotNull]
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in base.ConfigErrors() ?? Enumerable.Empty<string>())
            {
                yield return str;
            }

            if (resourceWorker != null && !resourceWorker.IsSubclassOf(typeof(ResourceWorker)))
            {
                yield return $"{resourceWorker} does not inherit from ResourceWorker!";
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            if (hillinessFactors == null)
            {
                yield return $"{nameof(hillinessFactors)} is null!";
            }

            if (hillinessOffsets == null)
            {
                yield return $"{nameof(hillinessOffsets)} is null!";
            }

            if (waterBodyFactors == null)
            {
                yield return $"{nameof(waterBodyFactors)} is null!";
            }

            if (waterBodyOffsets == null)
            {
                yield return $"{nameof(waterBodyOffsets)} is null!";
            }

            if (iconData is null)
            {
                yield return "No icon data!";
            }
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }
    }
}
