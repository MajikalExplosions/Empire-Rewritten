using Empire_Rewritten.Player;

namespace Empire_Rewritten.AI
{
    public abstract class AIModule
    {
        protected AIPlayer _player;

        public AIModule(AIPlayer player)
        {
            _player = player;
        }

        public abstract void DoModuleAction();
    }
}