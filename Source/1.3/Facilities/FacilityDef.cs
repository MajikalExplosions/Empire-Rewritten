using System;
using System.Collections.Generic;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using Verse;

namespace Empire_Rewritten.Facilities
{
    public class ResourceChange
    {
        public float amount;
        public ResourceDef def;
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class FacilityDef : Def
    {
        public readonly bool requiresIdeology;
        public readonly bool requiresRoyalty;

        [NotNull] [ItemNotNull] public readonly List<ResourceChange> resourceMultipliers = new List<ResourceChange>();
        [NotNull] [ItemNotNull] public readonly List<ResourceChange> resourceOffsets = new List<ResourceChange>();
        [NotNull] [ItemNotNull] public readonly List<string> requiredModIDs = new List<string>();

        [NotNull] public GraphicData iconData;
        [NotNull] [ItemNotNull] public List<ThingDefCountClass> costList;
        [CanBeNull] public Type facilityWorker;

        private FacilityWorker worker;

        [ItemNotNull] private List<ResourceDef> producedResources;

        [CanBeNull]
        public FacilityWorker FacilityWorker
        {
            get
            {
                if (facilityWorker == null) return null;
                return worker ?? (worker = (FacilityWorker)Activator.CreateInstance(facilityWorker, this));
            }
        }

        /// <summary>
        ///     Maintains a cache of the <see cref="ResourceDef">ResourceDefs</see> that are produced by this
        ///     <see cref="FacilityDef" />
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<ResourceDef> ProducedResources
        {
            get
            {
                if (producedResources != null) return producedResources;
                producedResources = new List<ResourceDef>();

                foreach (ResourceChange change in resourceOffsets)
                {
                    producedResources.Add(change.def);
                }

                foreach (ResourceChange change in resourceMultipliers)
                {
                    producedResources.Add(change.def);
                }

                return producedResources;
            }
        }

        /// <summary>
        ///     Whether or not all required mods and DLCs are installed and active.
        /// </summary>
        public bool RequiredModsLoaded => ModChecker.RequiredModsLoaded(requiredModIDs, requiresRoyalty, requiresIdeology);

        public override IEnumerable<string> ConfigErrors()
        {
            if (facilityWorker != null && !facilityWorker.IsSubclassOf(typeof(FacilityWorker)))
            {
                yield return $"{facilityWorker} does not inherit from FacilityWorker!";
            }

            if (base.ConfigErrors() is IEnumerable<string> baseErrors)
            {
                foreach (string str in baseErrors)
                {
                    yield return str;
                }
            }
        }
    }
}
