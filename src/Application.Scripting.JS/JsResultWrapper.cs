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

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var elementConvertible = typeof(IConvertible).IsAssignableFrom(elementType);
                if (obj is Array array)
                {
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
                    // 使用 List<T> 的构造函数直接传入数组
                    var list = Activator.CreateInstance(
                        typeof(List<>).MakeGenericType(elementType),
                        convertedArray
                    );
                    return (TObject)list;
                }
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
