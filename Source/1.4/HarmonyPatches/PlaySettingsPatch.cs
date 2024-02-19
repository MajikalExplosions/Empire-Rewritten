using HarmonyLib;
using RimWorld;
using Verse;

namespace Empire_Rewritten.HarmonyPatches
{
    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public static class PlaySettingsPatch
    {
        private static bool _showTerritories = true;

        public static bool ShowTerritories => _showTerritories;

        [HarmonyPostfix]
        public static void DoPlaySettingsGlobalControlsPatch(WidgetRow row, bool worldView)
        {
            if (worldView)
            {
                row.ToggleableIcon(ref _showTerritories, TexButton.ShowExpandingIcons, "Empire_ToggleTerritoriesView".TranslateSimple(), SoundDefOf.Mouseover_ButtonToggle);
            }
        }
    }
}
