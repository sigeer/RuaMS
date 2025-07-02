namespace Application.Module.Duey.Common
{
    public enum DueyProcessorActions
    {
        TOSERVER_RECV_ITEM = 0x00,
        TOSERVER_SEND_ITEM = 0x02,
        TOSERVER_CLAIM_PACKAGE = 0x04,
        TOSERVER_REMOVE_PACKAGE = 0x05,
        TOSERVER_CLOSE_DUEY = 0x07,
        TOCLIENT_OPEN_DUEY = 0x08,

        TOCLIENT_SEND_ENABLE_ACTIONS = 0x09,
        TOCLIENT_SEND_NOT_ENOUGH_MESOS = 0x0A,
        TOCLIENT_SEND_INCORRECT_REQUEST = 0x0B,
        TOCLIENT_SEND_NAME_DOES_NOT_EXIST = 0x0C,
        TOCLIENT_SEND_SAMEACC_ERROR = 0x0D,
        TOCLIENT_SEND_RECEIVER_STORAGE_FULL = 0x0E,
        TOCLIENT_SEND_RECEIVER_UNABLE_TO_RECV = 0x0F,
        TOCLIENT_SEND_RECEIVER_STORAGE_WITH_UNIQUE = 0x10,
        TOCLIENT_SEND_MESO_LIMIT = 0x11,
        TOCLIENT_SEND_SUCCESSFULLY_SENT = 0x12,

        TOCLIENT_RECV_UNKNOWN_ERROR = 0x13,
        TOCLIENT_RECV_ENABLE_ACTIONS = 0x14,
        TOCLIENT_RECV_NO_FREE_SLOTS = 0x15,
        TOCLIENT_RECV_RECEIVER_WITH_UNIQUE = 0x16,
        TOCLIENT_RECV_SUCCESSFUL_MSG = 0x17,
        TOCLIENT_RECV_PACKAGE_MSG = 0x1B
    }

    public static class ActionsExntesions
    {
        public static byte getCode(this DueyProcessorActions a)
        {
            return (byte)a;
        }
    }
}
