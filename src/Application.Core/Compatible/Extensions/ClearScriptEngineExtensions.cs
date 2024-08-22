using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace Application.Core.Compatible.Extensions
{
    public static class ClearScriptEngineExtensions
    {
        public static object InvokeSync(this V8ScriptEngine engine, string functionName, params object?[] args)
        {
            var r = engine.Invoke(functionName, args);
            if (r is Task<object> t)
            {
                return t.Result;
            }

            return r;
        }

        /// <summary>
        /// js没有返回值时，会返回<see cref="Undefined"/>。
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="functionName"></param>
        /// <param name="args"></param>
        /// <returns></returns>

        public static async Task<object> InvokeAsync(this V8ScriptEngine engine, string functionName, params object[] args)
        {
            var r = engine.Invoke(functionName, args);
            if (r is Task<object> t)
            {
                return await t;
            }

            return r;
        }
    }
}
