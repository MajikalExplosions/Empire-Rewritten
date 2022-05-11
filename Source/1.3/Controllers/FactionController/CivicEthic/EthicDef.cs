using System;
using System.Collections.Generic;
using System.Linq;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Empire_Rewritten.Controllers.CivicEthic
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class EthicDef : Def
    {
        public readonly bool requiresIdeology;
        public readonly bool requiresRoyalty;
        [NotNull] [ItemNotNull] public readonly List<string> requiredModIDs = new List<string>();
        [NotNull] [ItemNotNull] public readonly List<Type> abilityWorkers = new List<Type>();
        public List<EmpireStatModifier> statModifiers;

        public Type facilityWorker;

        /// <summary>
        ///     Whether all required mods and DLCs are loaded and active
        /// </summary>
        public bool RequiredModsLoaded => ModChecker.RequiredModsLoaded(requiredModIDs, requiresRoyalty, requiresIdeology);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (Type type in abilityWorkers.Where(type => !type.IsSubclassOf(typeof(EthicDef))))
            {
                yield return $"{type} does not inherit from EthicAbilityWorker!";
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

    [DefOf]
    public class EthicDefOf
    {
        [UsedImplicitly]
        static EthicDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EthicDefOf));
        }
    }
}
