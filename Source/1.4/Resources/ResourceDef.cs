using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Resources
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class ResourceDef : Def
    {
        public GraphicData iconData;
        public Graphic Graphic => iconData.Graphic;

        public Type resourceWorker;
        private ResourceWorker _worker;

        public bool alwaysShowInList = false;

        // Filters used to create a ThingFilter. These are from least to most specific, and are applied in that order.
        public List<StuffCategoryDef> allowedStuffCategoryDefs;
        public List<StuffCategoryDef> disallowedStuffCategoryDefs; // Technically useless (everything is disallowed by default) but kept for consistency

        public List<ThingCategoryDef> allowedThingCategoryDefs;
        public List<ThingCategoryDef> disallowedThingCategoryDefs;

        public List<ThingDef> allowedThingDefs;
        public List<ThingDef> disallowedThingDefs;

        private bool _hasThingFilter;
        private ThingFilter _thingFilter = new ThingFilter();


        // Modifiers from tile stats
        public SimpleCurve heightCurve;
        public SimpleCurve rainfallCurve;
        public SimpleCurve swampinessCurve;
        public SimpleCurve temperatureCurve;

        public TerrainFactors terrainFactors;
        public RiverFactors riverFactors;


        /// <summary>
        ///     Maintains a cached <see cref="ThingFilter" /> of the <see cref="ThingDef">resources</see> created from this
        ///     <see cref="ResourceDef" />
        /// </summary>
        public ThingFilter ThingFilter
        {
            get
            {
                if (_hasThingFilter) return _thingFilter;

                _thingFilter = new ThingFilter();
                _thingFilter.SetDisallowAll();

                allowedStuffCategoryDefs?.ForEach(category => _thingFilter.SetAllow(category, true));
                disallowedStuffCategoryDefs?.ForEach(category => _thingFilter.SetAllow(category, false));

                allowedThingCategoryDefs?.ForEach(category => _thingFilter.SetAllow(category, true));
                disallowedThingCategoryDefs?.ForEach(category => _thingFilter.SetAllow(category, false));

                allowedThingDefs?.ForEach(thingDef => _thingFilter.SetAllow(thingDef, true));
                disallowedThingDefs?.ForEach(thingDef => _thingFilter.SetAllow(thingDef, false));

                ResourceWorker?.PostModifyThingFilter();

                _hasThingFilter = true;

                return _thingFilter;
            }
        }

        /// <summary>
        ///     Maintains a cached <see cref="ResourceWorker" /> of the <see cref="ResourceDef.resourceWorker">specified Type</see>
        /// </summary>
        public ResourceWorker ResourceWorker
        {
            get
            {
                if (resourceWorker == null) return null;
                return _worker ?? (_worker = (ResourceWorker)Activator.CreateInstance(resourceWorker, _thingFilter));
            }
        }

        public Texture IconTexture => iconData.texPath.NullOrEmpty() ? null : ContentFinder<Texture2D>.Get(iconData.texPath);

        /// <summary>
        ///     Gets the <see cref="ResourceModifier" /> of a given <see cref="Tile" />
        /// </summary>
        /// <param name="tile">The <see cref="Tile" /> to get the <see cref="ResourceModifier" /> of</param>
        /// <returns>The <see cref="ResourceModifier" /> for <paramref name="tile" /></returns>
        public ResourceModifier GetTileModifier(Tile tile)
        {
            float heightMult = heightCurve.Evaluate(tile.elevation);
            float rainMult = rainfallCurve.Evaluate(tile.rainfall);
            float swampMult = swampinessCurve.Evaluate(tile.swampiness);
            float tempMult = temperatureCurve.Evaluate(tile.temperature);

            float terrainMult = terrainFactors.GetFactor(_GetTerrainType(tile));
            float riverMult = riverFactors.GetFactor(_GetRiverType(tile));

            ResourceModifier mod = new ResourceModifier(this, 0, heightMult * rainMult * swampMult * tempMult * terrainMult * riverMult);

            return mod;
        }

        private TerrainType _GetTerrainType(Tile tile)
        {
            TerrainType tt = TerrainType.Impassable;
            if (tile.hilliness == Hilliness.Flat)
            {
                tt = TerrainType.Flat;
            }
            else if (tile.hilliness == Hilliness.SmallHills)
            {
                tt = TerrainType.SmallHills;
            }
            else if (tile.hilliness == Hilliness.LargeHills)
            {
                tt = TerrainType.LargeHills;
            }
            else if (tile.hilliness == Hilliness.Mountainous)
            {
                tt = TerrainType.Mountainous;
            }
            else if (tile.hilliness == Hilliness.Impassable)
            {
                tt = TerrainType.Impassable;
            }
            else if (tile.WaterCovered)
            {
                tt = TerrainType.Ocean;
            }
            return tt;
        }

        private RiverType _GetRiverType(Tile tile)
        {
            RiverType rt = RiverType.None;
            RiverDef df = tile.Rivers?.MaxBy<Tile.RiverLink, int>((Func<Tile.RiverLink, int>)(riverlink => riverlink.river.degradeThreshold)).river;
            if (df == null) return rt;
            // No DefOf, so we use name
            if (df.defName == "Creek")
            {
                rt = RiverType.Creek;
            }
            else if (df.defName == "River")
            {
                rt = RiverType.River;
            }
            else if (df.defName == "LargeRiver")
            {
                rt = RiverType.LargeRiver;
            }
            else if (df.defName == "HugeRiver")
            {
                rt = RiverType.HugeRiver;
            }

            return rt;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (resourceWorker != null && !resourceWorker.IsSubclassOf(typeof(ResourceWorker)))
            {
                yield return $"{resourceWorker} does not inherit from ResourceWorker!";
            }

            foreach (string str in base.ConfigErrors())
            {
                yield return str;
            }
        }
    }
}
