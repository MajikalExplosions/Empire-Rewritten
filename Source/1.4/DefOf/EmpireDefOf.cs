using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using RimWorld;
using Verse;

namespace Empire_Rewritten
{
    [DefOf]
    public static class EmpireDefOf
    {
        static EmpireDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));

        public static ThingDef EmpireTaxSpot;

        public static TraderKindDef Empire_ResourceTrader;
        public static TraderKindDef Empire_TaxTrader;

        // TODO Consider RimWorld's resource types, like ThingCategoryDef

        // Basic Resources
        public static ResourceDef Empire_Res_Food;
        public static ResourceDef Empire_Res_Wood;
        public static ResourceDef Empire_Res_Stone;
        public static ResourceDef Empire_Res_Metal; // Can be converted from Stone, but at a loss
        public static ResourceDef Empire_Res_Textiles;

        // Intermediate Resources
        public static ResourceDef Empire_Res_Components; // From Metal
        public static ResourceDef Empire_Res_Apparel; // From Textiles
        public static ResourceDef Empire_Res_Drugs; // From Food, Textiles

        // Advanced Resources
        // public static ResourceDef Empire_Res_Weapons; // From Metal, Components, Wood
        // public static ResourceDef Empire_Res_Armor; // From Metal, Components, Textiles

        // Fallback Facility
        public static FacilityDef Empire_Fac_GatheringHut; // Produces Food, Wood, Stone

        // Basic (Starter) Facilities
        public static FacilityDef Empire_Fac_Farm; // Produces Food
        public static FacilityDef Empire_Fac_Lumberhut; // Produces Wood
        public static FacilityDef Empire_Fac_Quarry; // Produces Stone
        public static FacilityDef Empire_Fac_Mine; // Produces Metal
        public static FacilityDef Empire_Fac_Pasture; // Produces Food, Textiles
    }
}
