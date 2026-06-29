namespace server.movement
{
    public enum StanceConstant: byte
    {
        WalkR = 2,
        WalkL = 3,

        StandR,
        StandL,

        JumpR,
        JumpL,

        AfterBattleR,
        AfterBattleL,

        LadderR = 14,
        LadderL = 15,
        RopeR = 16,
        RopeL = 17
    }

    public class StanceUtils
    {
        public static bool IsBattle(byte stance)
        {
            return stance == (byte)StanceConstant.AfterBattleL || stance == (byte)StanceConstant.AfterBattleR;
        }

        public static bool IsOnLadderOrRope(byte stance)
        {
            return stance == (byte)StanceConstant.LadderR
                  || stance == (byte)StanceConstant.LadderL
                  || stance == (byte)StanceConstant.RopeR
                  || stance == (byte)StanceConstant.RopeL;
        }
    }
}
