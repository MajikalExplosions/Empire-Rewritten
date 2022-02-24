﻿using Empire_Rewritten.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Empire_Rewritten.AI
{
    public class AIResourceManager : AIModule
    {
        private static List<ResourceDef> cachedDefs = new List<ResourceDef>();
        private AIPlayer parentPlayer;

        private List<ResourceDef> criticalResources = new List<ResourceDef>();
        private int LowResourceDecider=30;
        private int HighResourceDecider = 50;

        public bool HasCriticalResource
        {
            get
            {
                return criticalResources.Count > 0;
            }
        }

        public AIResourceManager(AIPlayer player) : base(player)
        {
        }

        private List<ResourceDef> FindAllResourceDefs
        {
            get
            {
                if (cachedDefs.NullOrEmpty())
                {
                    cachedDefs = DefDatabase<ResourceDef>.AllDefsListForReading;
                }
                return cachedDefs;
            }
        }

        


        /// <summary>
        /// Figure out what resources are "low" in production based on the amount being produced.
        /// LowResourceDecider changes the low value.
        /// </summary>
        /// <returns>Any resource below LowResourceDecider, or the lowest resource.</returns>
        public List<ResourceDef> FindLowResources()
        {
            List<ResourceDef> result = new List<ResourceDef>();
            Dictionary<ResourceDef, float> producedknown = AllResourcesProduced();
            bool resourceBelowDecider = false;
           /* 
            Since producedknown is only useful if the AI produces the def
           the calculation checks against all ResourceDefs in the database
            */
            foreach(ResourceDef def in FindAllResourceDefs)
            {
                if (producedknown.ContainsKey(def))
                {
                    if (producedknown[def] <= LowResourceDecider)
                    {
                        result.Add(def);
                        resourceBelowDecider = true;
                        if (producedknown[def] < LowResourceDecider / 2)
                        {
                            criticalResources.Add(def);
                        }
                    }
                }
                //If the def is not produced at all, add it!
                else
                {
                    result.Add(def);
                    resourceBelowDecider = true;
                    criticalResources.Add(def);
                }
            }
            if (!resourceBelowDecider)
            {
                /*
                 Find the lowest defs produced, and adds them to a list.
                This isn't perfect, however it should catch most things.
                 */
                float lowest=0;
                bool first = true;
                foreach(ResourceDef def in producedknown.Keys)
                {
                    if (producedknown[def] <=lowest || (lowest == 0 && first))
                    {
                        first = false;
                        lowest = producedknown[def];
                        result.Add(def);
                    }
                }
            }
            return result;
        }



        /// <summary>
        /// Figure out what resources are "excess" in production based on the amount being produced.
        /// HighResourceDecider changes the excess value.
        /// </summary>
        /// <returns>Any resource below HighResourceDecider.</returns>
        public List<ResourceDef> FindExcessResources()
        {
            List<ResourceDef> result = new List<ResourceDef>();
            Dictionary<ResourceDef, float> producedknown = AllResourcesProduced();

            foreach (ResourceDef def in producedknown.Keys)
            {
                if (producedknown.ContainsKey(def))
                {
                    if (producedknown[def] >= HighResourceDecider)
                    {
                        result.Add(def);
                    }
                }
                //If the def is not produced at all, add it!
                else
                {
                    result.Add(def);
                }
            }
          
            return result;
        }

        /// <summary>
        /// Get amount of a resource produced
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public float GetAmountProduced(ResourceDef def)
        {
            Dictionary<ResourceDef, float> knownResources = AllResourcesProduced();
            float result = knownResources.ContainsKey(def) ? knownResources[def]: 0;
            return result;
        }

        /// <summary>
        /// Find all resources produced.
        /// </summary>
        /// <returns></returns>
        public Dictionary<ResourceDef, float> AllResourcesProduced()
        {
            Dictionary<ResourceDef, ResourceModifier> modifiers = parentPlayer.Manager.ResourceModifiersFromAllFacilities();
            Dictionary<ResourceDef, float> result = new Dictionary<ResourceDef, float>();
            foreach (ResourceDef def in modifiers.Keys)
            {
                ResourceModifier resourceModifier = modifiers[def];
                result.Add(def, resourceModifier.TotalProduced());
            }
            return result;
        }

        public override void DoModuleAction()
        {
            if (!criticalResources.EnumerableNullOrEmpty())
            {
                Dictionary<ResourceDef, float> resourcesProduced = AllResourcesProduced();
                criticalResources.RemoveAll(x => resourcesProduced.ContainsKey(x) && resourcesProduced[x] > LowResourceDecider / 2);
            }
        }
    }
}
