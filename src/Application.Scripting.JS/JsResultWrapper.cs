using Jint.Native;
using System.Data;

namespace Application.Scripting.JS
{
    public class JsResultWrapper : ScriptResultWrapper
    {
        JsValue? _value;

        public JsResultWrapper(JsValue? value)
        {
            _value = value;
        }

        public override TObject ToObject<TObject>()
        {
            if (_value == null)
                return default;

            var obj = _value.ToObject();
            var targetType = typeof(TObject);
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
                        convertedArray);
                    return (TObject)list;
                }
                return (TObject)(object)convertedArray;
            }

            return (TObject)obj;
        }

        public override object? ToObject()
        {
            return _value?.ToObject();
        }

        public override string ToString()
        {
            return _value?.ToString();
        }
    }
}
