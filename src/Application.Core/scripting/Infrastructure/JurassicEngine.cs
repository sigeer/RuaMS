//using Application.Core.Scripting.Infrastructure;
//using Jurassic;

//namespace Application.Core.scripting.Infrastructure
//{
//    public class JurassicEngine : IEngine
//    {
//        readonly ScriptEngine _engine;

//        public JurassicEngine()
//        {
//            _engine = new ScriptEngine()
//            {
//                EnableExposedClrTypes = true,
//            };
//        }

//        public void AddHostedObject(string name, object obj)
//        {
//            _engine.SetGlobalValue(name, obj);
//        }

//        public void AddHostedType(string name, Type type)
//        {
//            _engine.SetGlobalValue(name, type);
//        }

//        public object CallFunction(string functionName, params object[] paramsValue)
//        {
//            return _engine.CallGlobalFunction(functionName, paramsValue);
//        }

//        public object Evaluate(string code)
//        {
//            return _engine.Evaluate(code);
//        }
//    }
//}
