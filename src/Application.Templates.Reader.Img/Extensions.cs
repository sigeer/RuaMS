using Duey.Abstractions;
using System.Drawing;

namespace Application.Templates.Reader.Img
{
    public static class DataNodeExtensions
    {
        public static bool GetBoolValue(this IDataNode dataNode)
        {
            return dataNode.ResolveBool() ?? false;
        }

        public static Point GetVectorValue(this IDataNode dataNode)
        {
            var v = dataNode.ResolveVector();
            if (v == null) return Point.Empty;
            return new Point(v.Value.Item1, v.Value.Item2);
        }
        public static byte GetByteValue(this IDataNode dataNode, string? path = null, byte defaultValue = 0)
        {
            return (byte)GetLongValue(dataNode, path, defaultValue);
        }

        public static short GetShortValue(this IDataNode dataNode, string? path = null, short defaultValue = 0)
        {
            return (short)GetLongValue(dataNode, path, defaultValue);
        }

        public static int GetIntValue(this IDataNode dataNode, string? path = null, int defaultValue = 0)
        {
            return (int)GetLongValue(dataNode, path, defaultValue);
        }

        /// <summary>
        /// 代替ResolveLong.
        /// v83的wz中有些数据并不是严格，比如 &lt;string name="quest" value="7835.0000" />
        /// </summary>
        /// <param name="dataNode"></param>
        /// <param name="path"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long GetLongValue(this IDataNode dataNode, string? path = null, long defaultValue = 0)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var dest = dataNode.ResolvePath(path);
                if (dest == null)
                {
                    return defaultValue;
                }
                dataNode = dest;
            }
            if (dataNode is IDataProperty<long> interger)
            {
                return interger.Resolve();
            }
            else if (dataNode is IDataProperty<double> dec)
            {
                return (long)dec.Resolve();
            }
            else if (dataNode is IDataProperty<string> str)
            {
                return double.TryParse(str.Resolve(), out var v) ? (long)v : 0;
            }
            return 0;
        }

        public static float GetFloatValue(this IDataNode dataNode, string? path = null, float defaultValue = 0)
        {
            return (float)GetDoubleValue(dataNode, path, defaultValue);
        }

        public static double GetDoubleValue(this IDataNode dataNode, string? path = null, double defaultValue = 0)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var dest = dataNode.ResolvePath(path);
                if (dest == null)
                {
                    return defaultValue;
                }
                dataNode = dest;
            }

            if (dataNode is IDataProperty<long> interger)
            {
                return (double)interger.Resolve();
            }
            else if (dataNode is IDataProperty<double> dec)
            {
                return dec.Resolve();
            }
            else if (dataNode is IDataProperty<string> str)
            {
                return double.TryParse(str.Resolve().Replace(',', '.'), out var v) ? v : defaultValue;
            }
            return defaultValue;
        }

        public static string? GetStringValue(this IDataNode dataNode, string? path = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var dest = dataNode.ResolvePath(path);
                if (dest == null)
                {
                    return null;
                }
                dataNode = dest;
            }

            if (dataNode is IDataProperty<long> interger)
            {
                return interger.Resolve().ToString();
            }
            else if (dataNode is IDataProperty<double> dec)
            {
                return dec.Resolve().ToString();
            }
            else if (dataNode is IDataProperty<string> str)
            {
                return str.Resolve();
            }
            return null;
        }
    }
}
