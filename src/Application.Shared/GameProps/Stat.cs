using Application.Utility.Exceptions;

namespace Application.Shared.GameProps
{
    public enum Stat
    {
        SKIN = 0x1,
        FACE = 0x2,
        HAIR = 0x4,
        LEVEL = 0x10,
        JOB = 0x20,
        STR = 0x40,
        DEX = 0x80,
        INT = 0x100,
        LUK = 0x200,
        HP = 0x400,
        MAXHP = 0x800,
        MP = 0x1000,
        MAXMP = 0x2000,
        AVAILABLEAP = 0x4000,
        AVAILABLESP = 0x8000,
        EXP = 0x10000,
        FAME = 0x20000,
        MESO = 0x40000,
        PET = 0x180008,
        GACHAEXP = 0x200000
    }

    public static class StatUtils
    {
        public static Stat getByValue(int value)
        {
            return (Stat)value;
        }
        public static Stat getBy5ByteEncoding(int encoded)
        {
            return encoded switch
            {
                64 => Stat.STR,
                128 => Stat.DEX,
                256 => Stat.INT,
                512 => Stat.LUK,
                _ => throw new BusinessException(),
            };
        }

        public static Stat? getByString(string type)
        {
            if (Enum.TryParse<Stat>(type, out var d))
                return d;
            else
                return null;
        }
    }

    public static class StatExtensions
    {
        public static int getValue(this Stat i)
        {
            return (int)i;
        }
    }

}
