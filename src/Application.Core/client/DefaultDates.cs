namespace client;


public class DefaultDates
{
    // May 11 2005 is the date MapleGlobal released, so it's a symbolic default value

    private DefaultDates()
    {
    }

    public static DateTimeOffset getBirthday()
    {
        return DateTimeOffset.Parse("2005-05-11");
    }

    public static DateTimeOffset getTempban()
    {
        return DateTimeOffset.Parse("2005-05-11T00:00:00");
    }
}
