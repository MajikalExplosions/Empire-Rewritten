using System.Linq;
using System.Reflection;
using RimWorld;

namespace Empire_Rewritten.Player
{
    internal static class PlayerFactionGenerator
    {
        /// <summary>
        ///     Generate a player faction
        /// </summary>
        /// <returns></returns>
        public static Faction GeneratePlayerFaction()
        {
            Faction result = new Faction();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            foreach (FieldInfo playerFactionField in Faction.OfPlayer.GetType().GetFields(bindingFlags))
            {
                foreach (FieldInfo newFactionField in result.GetType().GetFields(bindingFlags).Where(field => field.Name == playerFactionField.Name))
                {
                    newFactionField?.SetValue(result, playerFactionField.GetValue(Faction.OfPlayer));
                }
            }

            return result;
        }
    }
}
