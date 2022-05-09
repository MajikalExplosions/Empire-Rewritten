using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Empire_Rewritten.Controllers;
using JetBrains.Annotations;

namespace Empire_Rewritten
{
    public static class PlayerHandler
    {
        [NotNull] [ItemNotNull] private static readonly List<BasePlayer> Players = new List<BasePlayer>();

        private static int _tick;

        private static bool _hasRegisteredUser;

        public static void Initialize(FactionController _)
        {
            _hasRegisteredUser = false;
            Players.Clear();

            UpdateController controller = UpdateController.CurrentWorldInstance;

            if (controller == null)
            {
                throw new NullReferenceException("Tried to initialize PlayerHandler without a valid UpdateController!");
            }

            controller.AddUpdateCall(MakeMoves, DoPlayerTick);
            controller.AddUpdateCall(MakeThreadedMoves, DoPlayerTick);
            controller.AddUpdateCall(RegisterPlayerFactionAsPlayer, ShouldRegisterPlayerFaction);
        }

        public static void RegisterPlayer([NotNull] BasePlayer player)
        {
            Players.Add(player);
        }

        private static bool DoPlayerTick()
        {
            if (_tick == 2)
            {
                _tick = 0;
                return true;
            }

            _tick++;
            return false;
        }

        public static void MakeMoves(FactionController controller)
        {
            foreach (BasePlayer player in Players)
            {
                if (player.ShouldExecute())
                {
                    player.MakeMove(controller);
                }
            }
        }

        public static void MakeThreadedMoves(FactionController controller)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                BasePlayer player = Players[i];

                void RunThreadedMove()
                {
                    if (player.ShouldExecuteThreaded())
                    {
                        player.MakeThreadedMove(controller);
                    }
                }

                Task.Run(RunThreadedMove);
            }
        }

        public static void RegisterPlayerFactionAsPlayer([NotNull] FactionController factionController)
        {
            factionController.CreatePlayer();
        }

        public static bool ShouldRegisterPlayerFaction()
        {
            bool result = !_hasRegisteredUser;
            _hasRegisteredUser = true;
            return result;
        }
    }
}
