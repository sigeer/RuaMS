namespace Application.Shared.Constants
{
    public enum PlayerInterAction
    {
        CREATE = 0,
        INVITE = 2,
        DECLINE = 3,
        VISIT = 4,
        ROOM = 5,
        CHAT = 6,
        CHAT_THING = 8,
        EXIT = 0xA,
        OPEN_STORE = 0xB,
        OPEN_CASH = 0xE,
        SET_ITEMS = 0xF,
        SET_MESO = 0x10,
        CONFIRM = 0x11,
        TRANSACTION = 0x14,
        ADD_ITEM = 0x16,
        BUY = 0x17,
        UPDATE_MERCHANT = 0x19,
        UPDATE_PLAYERSHOP = 0x1A,
        REMOVE_ITEM = 0x1B,
        BAN_PLAYER = 0x1C,
        MERCHANT_THING = 0x1D,
        OPEN_THING = 0x1E,
        PUT_ITEM = 0x21,
        MERCHANT_BUY = 0x22,
        TAKE_ITEM_BACK = 0x26,
        MAINTENANCE_OFF = 0x27,
        MERCHANT_ORGANIZE = 0x28,
        CLOSE_MERCHANT = 0x29,
        REAL_CLOSE_MERCHANT = 0x2A,
        MERCHANT_MESO = 0x2B,
        SOMETHING = 0x2D,
        VIEW_VISITORS = 0x2E,
        VIEW_BLACKLIST = 0x2F,
        ADD_TO_BLACKLIST = 0x30,
        REMOVE_FROM_BLACKLIST = 0x31,
        REQUEST_TIE = 0x32,
        ANSWER_TIE = 0x33,
        GIVE_UP = 0x34,
        EXIT_AFTER_GAME = 0x38,
        CANCEL_EXIT_AFTER_GAME = 0x39,
        READY = 0x3A,
        UN_READY = 0x3B,
        EXPEL = 0x3C,
        START = 0x3D,
        GET_RESULT = 0x3E,
        SKIP = 0x3F,
        MOVE_OMOK = 0x40,
        SELECT_CARD = 0x44
    }

    public static class PlayerInterActionExtensions
    {
        public static int getCode(this PlayerInterAction e) => (int)e;
    }
}
