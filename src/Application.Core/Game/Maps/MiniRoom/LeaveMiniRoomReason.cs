namespace Application.Core.Game.Maps.MiniRoom
{
    /// <summary>
    /// 
    /// </summary>
    public enum LeaveMiniRoomReason
    {
        /// <summary>
        /// 
        /// </summary>
        Blocked = 5,
        /// <summary>
        /// SP_3468_THE_OWNER_OF_THE_STORE_IS_CURRENTLY_R_NUNDERGOING_STORE_MAINTENANCE_R_NPLEASE_TR
        /// </summary>
        Maintenance = 17,
        /// <summary>
        /// SP_3463_IT_IS_PAST_THE_WORKING_HOURS_R_NSO_THE_STORE_WILL_CLOSE
        /// </summary>
        Expiration,
        /// <summary>
        /// SP_3469_THE_USE_OF_THE_REMOTE_CONTROL_STOPPED_R_NDUE_TO_MOVING_TO_ANOTHER_MAP_PLEASE_USE
        /// </summary>
        MapChange,
        /// <summary>
        /// SP_3464_YOUR_SHOP_WAS_CLOSED_AT_THE_DISCRETION_OF_A_GM_PLEASE_SEE_FRED_TO_RECOVER_YOUR_I
        /// </summary>
        DestoryByAdmin
    }
}
