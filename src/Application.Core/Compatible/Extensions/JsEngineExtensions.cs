using Jint.Native;
using Jint.Runtime.Interop;

namespace Application.Core.Compatible.Extensions
{
    public static class JsEngineExtensions
    {
        //public static object? InvokeSync(this IJsEngine engine, string functionName, params object?[] args)
        //{
        //    var r = engine.CallFunction(functionName, args);
        //    if (r is Task<object> t)
        //    {
        //        return t.Result;
        //    }

        //    return r;
        //}


        //public static TResult? InvokeFunction<TResult>(this IJsEngine engine, string functionName, params object?[] args)
        //{
        //    var r = engine.CallFunction(functionName, args);
        //    if (r is ObjectWrapper jsObject)
        //    {
        //        return (TResult)Convert.ChangeType(jsObject.ToObject(), typeof(TResult));
        //    }
        //    else
        //    {
        //        if (r is TResult d)
        //            return d;

        //        return default;
        //    }
        //}

        //public static List<TResult> InvokeFunctionList<TResult>(this IJsEngine engine, string functionName, params object?[] args)
        //{
        //    var r = engine.CallFunction(functionName, args);
        //    if (r is JsArray arr)
        //    {
        //        return arr.Select(x => (TResult)Convert.ChangeType(x.ToObject(), typeof(TResult))).ToList();
        //    }
        //    return [];
        //}

        ///// <summary>
        ///// js没有返回值时，会返回<see cref="Undefined"/>。
        ///// </summary>
        ///// <param name="engine"></param>
        ///// <param name="functionName"></param>
        ///// <param name="args"></param>
        ///// <returns></returns>

        //public static async Task<object> InvokeAsync(this IJsEngine engine, string functionName, params object[] args)
        //{
        //    var r = engine.CallFunction(functionName, args);
        //    if (r is Task<object> t)
        //    {
        //        return await t;
        //    }

        //    return r;
        //}
    }
}
