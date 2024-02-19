using Empire_Rewritten.Player;
using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;

namespace Empire_Rewritten
{
    public class PlayerController
    {
        private Dictionary<Faction, BasePlayer> _players = new Dictionary<Faction, BasePlayer>();
        public Dictionary<Faction, BasePlayer> Players
        {
            get => _players;
            private set => _players = value;
        }

        [UsedImplicitly]
        public PlayerController() { }

        public void CreateUserPlayer()
        {
            Faction faction = Faction.OfPlayer;
            Players.Add(faction, new UserPlayer(faction));
        }

        public void CreateAIPlayer(Faction faction)
        {
            Players.Add(faction, new AIPlayer(faction));
        }
    }
}