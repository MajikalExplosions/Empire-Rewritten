using Empire_Rewritten.Resources;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Settlements
{

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class FacilityDef : Def
    {
        public GraphicData iconData;
        public readonly List<string> requiredModIDs = new List<string>();
        public readonly bool requiresIdeology;
        public readonly bool requiresRoyalty;

        public ResearchProjectDef requiredResearch;

        public int buildDuration;
        public List<BuildCostEntry> buildCost;

        // TODO This is the base implementation, but I eventually want to change this to use a more complex system (e.g. Victoria 3's production methods)
        public List<ResourceModifier> production;
        public List<ResourceModifier> upkeep;

        public Type facilityWorker;
        private FacilityWorker _worker;

        public FacilityWorker FacilityWorker
        {
            get
            {
                if (facilityWorker == null) return null;
                return _worker ?? (_worker = (FacilityWorker)Activator.CreateInstance(facilityWorker, this));
            }
        }

        public Texture IconTexture => iconData.texPath.NullOrEmpty() ? null : ContentFinder<Texture2D>.Get(iconData.texPath);

        /// <summary>
        ///     Whether or not all required mods and DLCs are installed and active.
        /// </summary>
        public bool RequiredModsLoaded => ModChecker.RequiredModsLoaded(requiredModIDs, requiresRoyalty, requiresIdeology);

        public string GetCostString()
        {
            string res = "";
            foreach (BuildCostEntry cost in buildCost)
            {
                res += cost.amount + " x " + cost.resource.label + "; ";
            }

            return res.TrimEnd(',', ' ');
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (facilityWorker != null && !facilityWorker.IsSubclassOf(typeof(FacilityWorker)))
            {
                yield return $"{facilityWorker} does not inherit from FacilityWorker!";
            }

            foreach (string str in base.ConfigErrors())
            {
                yield return str;
            }
        }

        public class BuildCostEntry
        {
            public ResourceDef resource;
            public int amount;
        }
    }
}