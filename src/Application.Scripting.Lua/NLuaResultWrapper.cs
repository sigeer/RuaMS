
using NLua;

namespace Application.Scripting.Lua
{
    public class NLuaResultWrapper : ScriptResultWrapper
    {
        object[] _value;

        public NLuaResultWrapper(object[] value)
        {
            _value = value;
        }

        public override TypeCode GetTypeCode()
        {
            var obj = ToObject();
            if (obj == null)
                return TypeCode.Empty;

            return TypeCode.Object;
        }

        public override bool ToBoolean(IFormatProvider? provider)
        {
            return Convert.ToBoolean(ToObject());
        }

        public override byte ToByte(IFormatProvider? provider)
        {
            return Convert.ToByte(ToObject());
        }

        public override char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(ToObject());
        }

        public override DateTime ToDateTime(IFormatProvider? provider)
        {
            return Convert.ToDateTime(ToObject());
        }

        public override decimal ToDecimal(IFormatProvider? provider)
        {
            return Convert.ToDecimal(ToObject());
        }

        public override double ToDouble(IFormatProvider? provider)
        {
            return Convert.ToDouble(ToObject());
        }

        public override short ToInt16(IFormatProvider? provider)
        {
            return Convert.ToInt16(ToObject());
        }

        public override int ToInt32(IFormatProvider? provider)
        {
            return Convert.ToInt32(ToObject());
        }

        public override long ToInt64(IFormatProvider? provider)
        {
            return Convert.ToInt64(ToObject());
        }

        public override TObject ToObject<TObject>()
        {
            if (_value.Length == 0)
                return default;

            var d = _value[0];
            if (d is LuaTable table)
            {
                var targetType = typeof(TObject);

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
                var values = table.Values;
                var convertedArray = Array.CreateInstance(elementType, table.Values.Count);
                int index = 0;
                foreach (var item in table.Values)
                {
                    convertedArray.SetValue(
                         elementConvertible ?
                            Convert.ChangeType(item, elementType) :
                            item,
                        index
                    );
                    index++;
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
            else
            {
                return base.ToObject<TObject>(d);
            }
        }

        public override object? ToObject()
        {
            if (_value.Length == 0)
                return null;

            return _value[0];
        }

        public override sbyte ToSByte(IFormatProvider? provider)
        {
            return Convert.ToSByte(ToObject());
        }

        public override float ToSingle(IFormatProvider? provider)
        {
            return Convert.ToSingle(ToObject());
        }

        public override string ToString(IFormatProvider? provider)
        {
            return Convert.ToString(ToObject());
        }

        public override object ToType(Type conversionType, IFormatProvider? provider)
        {
            return Convert.ChangeType(ToObject(), conversionType);
        }

        public override ushort ToUInt16(IFormatProvider? provider)
        {
            return Convert.ToUInt16(ToObject());
        }

        public override uint ToUInt32(IFormatProvider? provider)
        {
            return Convert.ToUInt32(ToObject());
        }

        public override ulong ToUInt64(IFormatProvider? provider)
        {
            return Convert.ToUInt64(ToObject());
        }
    }
}
