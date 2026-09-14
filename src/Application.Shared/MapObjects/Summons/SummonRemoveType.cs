namespace Application.Shared.MapObjects.Summons
{
    public enum SummonRemoveType : byte
    {
        /// <summary>
        /// SP_3853_SS_TIME_HAS_RUN_OUT_AND_WILL_DISAPPEAR
        /// </summary>
        Timeout = 0,
        Dead = 1,
        /// <summary>
        /// v3 == 2 && ((v5 = *(this + 45), v5 == 3111002) || v5 == 3211002 || v5 == sub_C80EDC)
        /// </summary>
        Puppet = 2,
        /// <summary>
        /// SP_3854_S_IS_DISAPPEARING
        /// </summary>
        Disappearing = 3,
        Normal = 4
    }
}
