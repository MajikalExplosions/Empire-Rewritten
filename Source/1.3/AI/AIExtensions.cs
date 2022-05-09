using Empire_Rewritten.Controllers;
using JetBrains.Annotations;
using RimWorld;

namespace Empire_Rewritten.AI
{
    public static class AIExtensions
    {
        public static bool IsAIPlayer([NotNull] this Faction faction)
        {
            return GetAIPlayer(faction) != null;
        }

        [CanBeNull]
        public static AIPlayer GetAIPlayer([NotNull] this Faction faction)
        {
            return UpdateController.CurrentWorldInstance?.FactionController?.GetAIPlayer(faction);
        }
    }
}
