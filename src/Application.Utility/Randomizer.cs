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
        if (list == null) throw new ArgumentNullException(nameof(list));

        if (list is IList<TItem> listT)
        {
            if (listT.Count == 0)
                throw new InvalidOperationException("集合不能为空");
            return listT[Random.Shared.Next(listT.Count)];
        }

        var array = list.ToArray();
        if (array.Length == 0)
            throw new InvalidOperationException("集合不能为空");
        return array[Random.Shared.Next(array.Length)];
    }

    /// <summary>
    /// 从 [0, N-1] 中随机选取 M 个不重复的数。
    /// </summary>
    /// <param name="M">要选取的个数</param>
    /// <param name="N">范围上限（不包含 N）</param>
    /// <returns>包含 M 个随机不重复数的数组</returns>
    /// <exception cref="ArgumentException">当 M >= N 或 M < 0 时抛出</exception>
    public static int[] Take(int M, int N)
    {
        if (M < 0 || M >= N)
            throw new ArgumentException("M 必须大于等于 0 且小于 N");

        int[] result = Enumerable.Range(0, N).ToArray();
        Random.Shared.Shuffle(result); // .NET 8+ 有内置 Shuffle
        return result[..M];
    }

}