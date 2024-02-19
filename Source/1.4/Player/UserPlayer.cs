using RimWorld;

namespace Empire_Rewritten.Player
{
    public class UserPlayer : BasePlayer
    {
        public UserPlayer(Faction faction) : base(faction) { }


        public override void MakeMove(EmpireWorldComp worldComp)
        {
            // No moves for player, as they will interact with UI instead.
            return;
            //throw new NotImplementedException();
        }

        public override bool IsAI()
        {
            return false;
        }
    }
}