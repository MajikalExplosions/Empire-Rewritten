using System.Collections.Generic;
using Empire_Rewritten.Resources;
using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.AI
{
    public class AIResourceManager : AIModule
    {
        public AIResourceManager([NotNull] AIPlayer player) : base(player) { }

        [NotNull] [ItemNotNull] public List<ResourceDef> CriticalResources { get; } = new List<ResourceDef>();
        [NotNull] [ItemNotNull] public List<ResourceDef> ExcessResources { get; } = new List<ResourceDef>();
        [NotNull] [ItemNotNull] public List<ResourceDef> LowResources { get; } = new List<ResourceDef>();

        public bool HasCriticalResource => CriticalResources.Count > 0;

        /// <summary>
        ///     Figure out what resources are "low" in production based on the amount being produced.
        ///     LowResourceDecider changes the low value.
        /// </summary>
        /// <returns>Any resource below LowResourceDecider, or the lowest resource.</returns>
        [NotNull]
        public List<ResourceDef> FindLowResources()
        {
            List<ResourceDef> result = new List<ResourceDef>();
            Dictionary<ResourceDef, float> producedKnown = AllResourcesProduced();
            bool resourceBelowDecider = false;
            /* 
             Since producedKnown is only useful if the AI produces the def
            the calculation checks against all ResourceDefs in the database
             */
            foreach (ResourceDef def in DefDatabase<ResourceDef>.AllDefsListForReading)
            {
                if (producedKnown.ContainsKey(def))
                {
                    if (producedKnown[def] <= def.desiredAIMinimum)
                    {
                        result.Add(def);
                        resourceBelowDecider = true;
                        if (producedKnown[def] < def.desiredAIMinimum / 2f)
                        {
                            CriticalResources.Add(def);
                        }
                    }
                }
                //If the def is not produced at all, add it!
                else
                {
                    result.Add(def);
                    resourceBelowDecider = true;
                    CriticalResources.Add(def);
                }
            }

            if (!resourceBelowDecider)
            {
                /*
                 Find the lowest defs produced, and adds them to a list.
                This isn't perfect, however it should catch most things.
                 */
                float lowest = 0;
                bool first = true;
                foreach ((ResourceDef def, float produced) in producedKnown)
                {
                    if (produced <= lowest || (lowest == 0 && first))
                    {
                        first = false;
                        lowest = produced;
                        result.Add(def);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Figure out what resources are "excess" in production based on the amount being produced.
        ///     HighResourceDecider changes the excess value.
        /// </summary>
        /// <returns>Any resource below HighResourceDecider.</returns>
        [NotNull]
        [ItemNotNull]
        public List<ResourceDef> FindExcessResources()
        {
            List<ResourceDef> result = new List<ResourceDef>();
            Dictionary<ResourceDef, float> producedKnown = AllResourcesProduced();

            foreach ((ResourceDef def, float produced) in producedKnown)
            {
                if (produced >= def?.desiredAIMaximum)
                {
                    result.Add(def);
                }
            }

            return result;
        }

        /// <summary>
        ///     Get amount of a resource produced
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public float GetAmountProduced(ResourceDef def)
        {
            return AllResourcesProduced().TryGetValue(def);
        }

        /// <summary>
        ///     Find all resources produced.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public Dictionary<ResourceDef, float> AllResourcesProduced()
        {
            Dictionary<ResourceDef, float> result = new Dictionary<ResourceDef, float>();
            if (player.Empire == null) return result;

            Dictionary<ResourceDef, ResourceModifier> modifiers = player.Empire.ResourceModifiersFromAllFacilities();
            foreach (ResourceDef def in DefDatabase<ResourceDef>.AllDefsListForReading)
            {
                if (modifiers.ContainsKey(def))
                {
                    ResourceModifier resourceModifier = modifiers[def];
                    result.Add(def, resourceModifier.TotalProduced());
                }
                else
                {
                    result.Add(def, 0);
                }
            }

            return result;
        }

        public override void DoModuleAction() { }

        /// <summary>
        ///     Search for tiles to build settlements on based off weights;
        ///     Weights:
        ///     - Resources
        ///     - Territory distance
        ///     Resources AI wants = higher weight
        ///     Resources AI has excess of = lower weight
        /// </summary>
        /// <returns></returns>
        public float GetTileResourceWeight([NotNull] Tile tile)
        {
            float weight = 0;
            foreach (ResourceDef resourceDef in LowResources)
            {
                weight += resourceDef.GetTileModifier(tile).TotalProduced();
            }

            foreach (ResourceDef resourceDef in CriticalResources)
            {
                weight += resourceDef.GetTileModifier(tile).TotalProduced() * 5;
            }

            foreach (ResourceDef resourceDef in ExcessResources)
            {
                weight -= resourceDef.GetTileModifier(tile).TotalProduced() * 3;
            }

            return weight;
        }

        public override void DoThreadableAction()
        {
            if (!CriticalResources.EnumerableNullOrEmpty())
            {
                Dictionary<ResourceDef, float> resourcesProduced = AllResourcesProduced();
                CriticalResources.RemoveAll(resource => resource != null && resourcesProduced.TryGetValue(resource) > resource.desiredAIMinimum / 2f);
            }

            LowResources.Clear();
            LowResources.AddRange(FindLowResources());
            ExcessResources.Clear();
            FindExcessResources();
        }
    }
}
