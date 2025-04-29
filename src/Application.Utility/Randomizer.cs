namespace Application.Utility;

public class Randomizer
{
    /// <summary>
    /// [int.MinValue, int.MaxValue)
    /// 和java的nextInt仍有区别
    /// </summary>
    /// <returns></returns>
    public static int nextInt()
    {
        return Random.Shared.Next(int.MinValue, int.MaxValue);
    }

    /// <summary>
    /// [0, arg0)
    /// </summary>
    /// <param name="arg0"></param>
    /// <returns></returns>
    public static int nextInt(int arg0)
    {
        return Random.Shared.Next(arg0);
    }

    /// <summary>
    /// [min, max) 包含最小值，但不包含最大值
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int NextInt(int min, int max)
    {
        return Random.Shared.Next(min, max);
    }

    public static short NextShort()
    {
        return (short)Random.Shared.Next(short.MaxValue);
    }

    public static void nextBytes(byte[] bytes)
    {
        Random.Shared.NextBytes(bytes);
    }

    public static bool nextBoolean()
    {
        return nextDouble() < 0.5;
    }

    public static double nextDouble()
    {
        return Random.Shared.NextDouble();
    }

    public static float NextFloat()
    {
        return Random.Shared.NextSingle();
    }

    public static long nextLong()
    {
        return Random.Shared.NextInt64();
    }

    /// <summary>
    /// [lbound, ubound]，包含最小值和最大值
    /// </summary>
    /// <param name="lbound"></param>
    /// <param name="ubound"></param>
    /// <returns></returns>
    public static int rand(int lbound, int ubound)
    {
        return Random.Shared.Next(lbound, ubound + 1);
    }

    public static TItem Select<TItem>(IEnumerable<TItem> list)
    {
        var len = list.Count();
        return list.ElementAt(nextInt(len));
    }
}