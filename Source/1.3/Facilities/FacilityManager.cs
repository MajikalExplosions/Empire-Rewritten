﻿using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

namespace Empire_Rewritten.Facilities
{
    /// <summary>
    ///     This manages all the <see cref="Facility">Facilities</see> for a <see cref="Settlement" />.
    ///     It also manages the <see cref="ResourceModifier">ResourceModifiers</see> from each <see cref="Facility" />.
    /// </summary>
    public class FacilityManager : IExposable
    {
        private readonly bool refreshFacilityCount = true;
        [NotNull] [ItemNotNull] private readonly List<Gizmo> gizmos = new List<Gizmo>();
        private bool refreshGizmos = true;
        private bool refreshModifiers = true;
        [NotNull] private Dictionary<FacilityDef, Facility> installedFacilities = new Dictionary<FacilityDef, Facility>();
        private int facilityCount;
        private int stage = 1;
        [NotNull] private List<ResourceModifier> cachedModifiers = new List<ResourceModifier>();
        [NotNull] private Settlement settlement;

        public FacilityManager([NotNull] Settlement settlement)
        {
            this.settlement = settlement;
        }

        [UsedImplicitly]
        public FacilityManager() { }

        /// <summary>
        ///     All <see cref="ResourceModifier">ResourceModifiers</see> from installed <see cref="Facility">Facilities</see>.
        /// </summary>
        [NotNull]
        public IEnumerable<ResourceModifier> Modifiers
        {
            get
            {
                if (!refreshModifiers) return cachedModifiers;

                UpdateModiferCache();
                refreshModifiers = false;

                return cachedModifiers;
            }
        }

        /// <summary>
        ///     Installed <see cref="FacilityDef">FacilityDefs</see>
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IEnumerable<FacilityDef> FacilityDefsInstalled => installedFacilities.Keys;

        public bool IsFullyUpgraded => stage >= 10;

        public int MaxFacilities => stage;

        public int FacilityCount
        {
            get
            {
                if (refreshFacilityCount)
                {
                    int count = 0;
                    foreach (Facility facility in installedFacilities.Values)
                    {
                        count += facility?.Amount ?? 0;
                    }

                    facilityCount = count;
                }

                return facilityCount;
            }
        }

        public bool CanBuildNewFacilities => FacilityCount < MaxFacilities;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref installedFacilities, "installedFacilities", LookMode.Deep, LookMode.Deep);
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_Values.Look(ref stage, "stage");
        }

        /// <summary>
        ///     Get gizmos from all facilities in the settlement.
        /// </summary>
        [ItemNotNull]
        public IEnumerable<Gizmo> GetGizmos()
        {
            if (!refreshGizmos) return gizmos;

            gizmos.Clear();
            refreshGizmos = false;
            foreach (Facility facility in installedFacilities.Values)
            {
                if (facility == null) continue;
                gizmos.AddRange(facility.FacilityWorker?.GetGizmos() ?? Enumerable.Empty<Gizmo>());
            }

            return gizmos;
        }

        /// <summary>
        ///     Refreshes the <see cref="FacilityManager.cachedModifiers" />.
        /// </summary>
        private void UpdateModiferCache()
        {
            Dictionary<ResourceDef, ResourceModifier> calculatedModifiers = new Dictionary<ResourceDef, ResourceModifier>();

            foreach (Facility facility in installedFacilities.Values)
            {
                if (facility == null) continue;

                foreach (ResourceModifier modifier in facility.ResourceModifiers)
                {
                    if (calculatedModifiers.ContainsKey(modifier.def))
                    {
                        ResourceModifier newModifier = calculatedModifiers[modifier.def].MergeWithModifier(modifier);
                        calculatedModifiers[modifier.def] = newModifier;
                    }
                    else
                    {
                        calculatedModifiers.Add(modifier.def, modifier);
                    }
                }
            }

            cachedModifiers = calculatedModifiers.Values.ToList();
        }

        /// <summary>
        ///     Invalidates cached <see cref="Gizmo">Gizmos</see> and <see cref="ResourceModifier">ResourceModifiers</see>
        /// </summary>
        /// <param name="shouldRefreshGizmos">
        ///     Whether to refresh <see cref="FacilityManager.gizmos" /> alongside <see cref="FacilityManager.cachedModifiers" />
        /// </param>
        public void SetDataDirty(bool shouldRefreshGizmos = false)
        {
            refreshGizmos = shouldRefreshGizmos;
            refreshModifiers = true;
        }

        /// <summary>
        ///     Adds a new <see cref="Facility" /> of a given <see cref="FacilityDef" /> to this <see cref="FacilityManager" />'s
        ///     <see cref="FacilityManager.settlement" />
        /// </summary>
        /// <param name="facilityDef">The <see cref="FacilityDef" /> to add</param>
        public void AddFacility([NotNull] FacilityDef facilityDef)
        {
            if (installedFacilities.ContainsKey(facilityDef))
            {
                installedFacilities[facilityDef]?.AddFacility();
            }
            else
            {
                installedFacilities.Add(facilityDef, new Facility(facilityDef, settlement));
                SetDataDirty(true);
            }
        }

        /// <summary>
        ///     Removes a new <see cref="Facility" /> of a given <see cref="FacilityDef" /> from this
        ///     <see cref="FacilityManager" />'s <see cref="FacilityManager.settlement" />
        /// </summary>
        /// <param name="facilityDef">The <see cref="FacilityDef" /> to remove</param>
        public void RemoveFacility([NotNull] FacilityDef facilityDef)
        {
            Facility installed = installedFacilities.TryGetValue(facilityDef);
            if (installed == null) return;

            installed.RemoveFacility();
            if (installed.Amount <= 0)
            {
                if (installed.Amount < 0)
                {
                    Logger.Warn("FacilityManager.RemoveFacility: Amount of " + facilityDef.defName + " is negative!");
                }

                installedFacilities.Remove(facilityDef);
                SetDataDirty(true);
            }
        }

        /// <summary>
        ///     Checks whether this <see cref="FacilityManager" /> has a <see cref="Facility" /> of a given
        ///     <see cref="FacilityDef" />
        /// </summary>
        /// <param name="facilityDef">The <see cref="FacilityDef" /> to check for</param>
        /// <returns>Whether <paramref name="facilityDef" /> is installed here</returns>
        public bool HasFacility([CanBeNull] FacilityDef facilityDef)
        {
            return facilityDef != null && installedFacilities.ContainsKey(facilityDef);
        }
    }
}
