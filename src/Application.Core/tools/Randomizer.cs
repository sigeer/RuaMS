namespace tools;

public class Randomizer
{

    private static Random _rand = new Random();

    /// <summary>
    /// [0, int.MaxValue)
    /// </summary>
    /// <returns></returns>
    public static int nextInt()
    {
        return _rand.Next();
    }

    /// <summary>
    /// [0, arg0)
    /// </summary>
    /// <param name="arg0"></param>
    /// <returns></returns>
    public static int nextInt(int arg0)
    {
        return _rand.Next(arg0);
    }

    public static short NextShort()
    {
        return (short)_rand.Next(short.MaxValue);
    }

    public static void nextBytes(byte[] bytes)
    {
        _rand.NextBytes(bytes);
    }

    public static bool nextBoolean()
    {
        return nextDouble() < 0.5;
    }

    public static double nextDouble()
    {
        return _rand.NextDouble();
    }

    public static float nextFloat()
    {
        return _rand.NextSingle();
    }

    public static long nextLong()
    {
        return _rand.NextInt64();
    }

    /// <summary>
    /// [lbound, ubound]
    /// </summary>
    /// <param name="lbound"></param>
    /// <param name="ubound"></param>
    /// <returns></returns>
    public static int rand(int lbound, int ubound)
    {
        return ((int)(_rand.NextDouble() * (ubound - lbound + 1))) + lbound;
    }

    public static TItem Select<TItem>(IEnumerable<TItem> list)
    {
        var len = list.Count();
        return list.ElementAt(nextInt(len));
    }
}