namespace Application.Scripting
{
    public abstract class ScriptResultWrapper: IConvertible
    {
        public abstract TypeCode GetTypeCode();
        public abstract bool ToBoolean(IFormatProvider? provider);
        public abstract byte ToByte(IFormatProvider? provider);
        public abstract char ToChar(IFormatProvider? provider);
        public abstract DateTime ToDateTime(IFormatProvider? provider);
        public abstract decimal ToDecimal(IFormatProvider? provider);
        public abstract double ToDouble(IFormatProvider? provider);
        public abstract short ToInt16(IFormatProvider? provider);
        public abstract int ToInt32(IFormatProvider? provider);
        public abstract long ToInt64(IFormatProvider? provider);
        public abstract sbyte ToSByte(IFormatProvider? provider);
        public abstract float ToSingle(IFormatProvider? provider);
        public abstract string ToString(IFormatProvider? provider);
        public abstract object ToType(Type conversionType, IFormatProvider? provider);
        public abstract ushort ToUInt16(IFormatProvider? provider);
        public abstract uint ToUInt32(IFormatProvider? provider);
        public abstract ulong ToUInt64(IFormatProvider? provider);



        public abstract TObject? ToObject<TObject>();
        public abstract object? ToObject();
        protected virtual TObject ToObject<TObject>(object? obj)
        {
            if (obj == null)
                return default;

            var targetType = typeof(TObject);
            if (targetType.IsEnum)
                return (TObject)Enum.ToObject(targetType, Convert.ToInt32(obj));
            if (typeof(IConvertible).IsAssignableFrom(targetType))
                return (TObject)Convert.ChangeType(obj, targetType);

            if (obj is Array array)
            {
                Type? elementType = null;
                if (targetType.IsArray)
                    elementType = targetType.GetElementType();

                bool isList = false;
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    elementType = targetType.GetGenericArguments()[0];
                    isList = true;
                }

                if (elementType == null)
                    throw new InvalidCastException();

                var elementConvertible = typeof(IConvertible).IsAssignableFrom(elementType);
                var convertedArray = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    var item = array.GetValue(i);
                    convertedArray.SetValue(
                         elementConvertible ?
                            Convert.ChangeType(item, elementType) :
                            item,
                        i
                    );
                }

                if (isList)
                {
                    var list = Activator.CreateInstance(
                        typeof(List<>).MakeGenericType(elementType),
                        convertedArray)!;
                    return (TObject)list;
                }
                return (TObject)(object)convertedArray;
            }

            return (TObject)obj;
        }
    }
}
