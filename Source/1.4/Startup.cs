using Empire_Rewritten.HarmonyPatches;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using Verse;

namespace Empire_Rewritten
{
    /// <summary>
    ///     This class handles the mod's startup needs.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Startup
    {
        /// <summary>
        ///     Gets called on startup
        /// </summary>
        [UsedImplicitly]
        static Startup()
        {
            Logger.Message("[Empire] just here to say hello! ^-^ Have a nice day and great fun with Empire!".Rainbowify(' ', 35));
            HarmonyPatcher.DoPatches();
        }
    }
}