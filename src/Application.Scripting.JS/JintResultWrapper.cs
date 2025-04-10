using Jint;
using Jint.Native;

namespace Application.Scripting.JS
{
    public class JintResultWrapper : ScriptResultWrapper
    {
        public JsValue Value { get; }

        public JintResultWrapper(JsValue value)
        {
            Value = value;
        }

        public override object? ToObject()
        {
            return Value.ToObject();
        }

        public override TObject ToObject<TObject>()
        {
            if (Value.IsNull())
                return default;

            var obj = Value.ToObject();
            return base.ToObject<TObject>(obj);
        }

        public override TypeCode GetTypeCode()
        {
            return Value.Type.GetTypeCode();
        }

        public override bool ToBoolean(IFormatProvider? provider)
        {
            return Value.AsBoolean();
        }

        public override byte ToByte(IFormatProvider? provider)
        {
            return (byte)Value.AsNumber();
        }

        public override char ToChar(IFormatProvider? provider)
        {
            return Value.AsString()[0];
        }

        public override DateTime ToDateTime(IFormatProvider? provider)
        {
            return Value.AsDate().ToDateTime();
        }

        public override decimal ToDecimal(IFormatProvider? provider)
        {
            return (decimal)Value.AsNumber();
        }

        public override double ToDouble(IFormatProvider? provider)
        {
            return (double)Value.AsNumber();
        }

        public override short ToInt16(IFormatProvider? provider)
        {
            return (short)Value.AsNumber();
        }

        public override int ToInt32(IFormatProvider? provider)
        {
            return (int)Value.AsNumber();
        }

        public override long ToInt64(IFormatProvider? provider)
        {
            return (long)Value.AsNumber();
        }

        public override sbyte ToSByte(IFormatProvider? provider)
        {
            return (sbyte)Value.AsNumber();
        }

        public override float ToSingle(IFormatProvider? provider)
        {
            return (float)Value.AsNumber();
        }

        public override string ToString()
        {
            return Value.AsString();
        }

        public override string ToString(IFormatProvider? provider)
        {
            return Value.AsString();
        }

        public override object ToType(Type conversionType, IFormatProvider? provider)
        {
            return Convert.ChangeType(Value.AsObject(), conversionType);
        }

        public override ushort ToUInt16(IFormatProvider? provider)
        {
            return (ushort)Value.AsNumber();
        }

        public override uint ToUInt32(IFormatProvider? provider)
        {
            return (uint)Value.AsNumber();
        }

        public override ulong ToUInt64(IFormatProvider? provider)
        {
            return (ulong)Value.AsNumber();
        }
    }
}
