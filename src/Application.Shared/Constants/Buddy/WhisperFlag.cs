namespace Application.Shared.Constants.Buddy
{
    public static class WhisperFlag
    {
        public const byte LOCATION = 0x01;
        public const byte WHISPER = 0x02;
        public const byte REQUEST = 0x04;
        public const byte RESULT = 0x08;
        public const byte RECEIVE = 0x10;
        public const byte BLOCKED = 0x20;
        public const byte LOCATION_FRIEND = 0x40;
    }
}
