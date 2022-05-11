using Empire_Rewritten.Controllers;
using Empire_Rewritten.Settlements;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Windows
{
    public static class DebugActions
    {
        [PublicAPI]
        [DebugAction("Empire", "Resource info window", allowedGameStates = AllowedGameStates.Entry)]
        public static void DisplayResourceInfoWindow()
        {
            Find.WindowStack?.Add(new ResourceInfoWindow());
        }

        [PublicAPI]
        [DebugAction("Empire", "Facility info window", allowedGameStates = AllowedGameStates.Entry)]
        public static void FacilityInfoWindow()
        {
            Find.WindowStack?.Add(new FacilityInfoWindow());
        }

        [PublicAPI]
        [DebugAction("Empire", "Color picker window", allowedGameStates = AllowedGameStates.Entry)]
        public static void ColorPickerWindow()
        {
            Color color = Color.green;
            Color[] colors = new Color[10];
            Find.WindowStack?.Add(new ColorPickerWindow(color, colors, _ => { }, _ => { }));
        }

        [PublicAPI]
        [DebugAction("Empire", "Player empire creation window", allowedGameStates = AllowedGameStates.Playing)]
        public static void PlayerCreationWindow()
        {
            Find.WindowStack?.Add(new PlayerFactionCreationWindow());
        }

        [PublicAPI]
        [DebugAction("Empire", "Settlement creation window", allowedGameStates = AllowedGameStates.Playing)]
        public static void SettlementCreationWindow()
        {
            Find.WindowStack?.Add(new SettlementPlacementWindow());
        }

        [PublicAPI]
        [DebugAction("Empire", "Item transfer window", allowedGameStates = AllowedGameStates.Playing)]
        public static void ItemTransferWindow()
        {
            Find.WindowStack?.Add(new ItemTransferWindow());
        }

        [PublicAPI]
        [DebugAction("Empire", "Fill storage with settlement cost", allowedGameStates = AllowedGameStates.Playing)]
        public static void FillStorage()
        {
            foreach ((ThingDef thing, int amount) in Empire.SettlementCost)
            {
                if (thing is null) continue;
                UpdateController.CurrentWorldInstance?.FactionController?.ReadOnlyFactionSettlementData
                                .Find(settlementData => settlementData != null && !settlementData.Empire.IsAIPlayer)
                                ?.Empire.StorageTracker.AddThingsToStorage(thing, amount);
            }
        }
    }
}
