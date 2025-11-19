using Application.Scripting;
using Application.Scripting.JS;
using System.Drawing;
using System.Text;

namespace ServiceTest.Infrastructure.Scripts
{
    public abstract class BaseScriptTest
    {
        protected IEngine _engine;
        protected string Code;

        public virtual void GetValueTest()
        {
            _engine.Evaluate(Code);

            Assert.That(_engine.GetValue("v1").ToObject<int>(), Is.EqualTo(1));
            Assert.That(_engine.GetValue("v2").ToObject<string>(), Is.EqualTo("2"));
            Assert.That(_engine.GetValue("v3").ToObject<string>(), Is.EqualTo("v3"));
            Assert.That(_engine.GetValue("v4").ToObject<double>(), Is.EqualTo(1.4));
            Assert.That(_engine.GetValue("v5").ToObject(), Is.EqualTo(null));
            Assert.That(_engine.GetValue("v6").ToObject(), Is.EqualTo(null));
        }

        public virtual void CheckMathRandomTest()
        {
            _engine.Evaluate(Code);
            var d = _engine.CallFunction("test").ToObject<double>();
            Assert.That(d > 0 && d < 1);
            var d2 = _engine.CallFunction("test").ToObject<double>();
            Assert.That(d2 > 0 && d2 < 1 && d != d2);
        }

        /// <summary>
        /// 返回脚本中定义的数组
        /// </summary>
        public virtual void ReturnScriptArray()
        {
            _engine.Evaluate(Code);

            var toInt32Array = _engine.CallFunction("test").ToObject<int[]>();
            Assert.That(toInt32Array.Sum(), Is.EqualTo(16));

            var toDouble32Array = _engine.CallFunction("test").ToObject<double[]>();
            Assert.That(toInt32Array.Sum(), Is.EqualTo(16));
        }
        /// <summary>
        /// 返回脚本中定义的int
        /// </summary>
        public virtual void ReturnScriptInt()
        {
            _engine.Evaluate(Code);

            var test1 = _engine.CallFunction("test").ToObject<int>();
            // 会自行四舍五入
            Assert.That(test1, Is.EqualTo(1235));

            var test2 = _engine.CallFunction("test").ToObject<double>();
            Assert.That(test2, Is.EqualTo(1234.56));
        }
        /// <summary>
        /// 返回脚本中定义的double
        /// </summary>
        public virtual void ReturnScriptDouble()
        {
            _engine.Evaluate(Code);

            var test = _engine.CallFunction("test").ToObject<double>();
            Assert.That(test, Is.EqualTo(1.234));
        }

        /// <summary>
        /// 返回脚本中定义的bool
        /// </summary>
        public virtual void ReturnScriptBoolean()
        {
            _engine.Evaluate(Code);

            var test_true = _engine.CallFunction("test_true").ToObject<bool>();
            var test_false = _engine.CallFunction("test_false").ToObject<bool>();
            var test = _engine.CallFunction("test").ToObject<bool>();
            Assert.That(test_true);
            Assert.That(test_false, Is.EqualTo(false));
            Assert.That(test, Is.EqualTo(false));
        }

        /// <summary>
        /// 返回脚本中实例化的新对象
        /// </summary>
        public virtual void ReturnScriptNewObject()
        {
            _engine.AddHostedType("ScriptTestClass", typeof(ScriptTestClass));
            _engine.Evaluate(Code);

            var test1 = _engine.CallFunction("test").ToObject<ScriptTestClass>();
            Assert.That(test1!.ShortValue, Is.EqualTo(1));
        }

        /// <summary>
        /// 返回脚本中创建的对象数组
        /// </summary>
        public virtual void ReturnScriptNewObjectArray()
        {
            _engine.AddHostedType("ScriptTestClass", typeof(ScriptTestClass));
            _engine.AddHostedType("ScriptTestStaticClass", typeof(ScriptTestStaticClass));
            _engine.Evaluate(Code);

            var check_returnResult = _engine.CallFunction("test");

            var test1 = check_returnResult.ToObject<ScriptTestClass[]>();
            Assert.That(test1!.Length, Is.EqualTo(3));

            var test2 = check_returnResult.ToObject<List<ScriptTestClass>>();
            Assert.That(test2!.Count(), Is.EqualTo(3));
        }

        /// <summary>
        /// 脚本中修改传入的C#对象属性
        /// </summary>
        public virtual void UpdateObject()
        {
            _engine.Evaluate(Code);

            var obj = new ScriptTestClass();
            var test1 = _engine.CallFunction("test", new object[] { obj }).ToObject<ScriptTestClass>();
            Assert.That(obj.ShortValue, Is.EqualTo(999));
            Assert.That(test1 == obj);
        }

        /// <summary>
        /// 脚本中修改传入的C#数组
        /// </summary>
        public virtual void UpdateArray()
        {
            _engine.Evaluate(Code);

            int[] arr = [1, 2, 3, 4];

            var test1 = _engine.CallFunction("test", new object[] { arr }).ToObject<int[]>();
            Assert.That(test1[1], Is.EqualTo(100));
            // Jint ToObject内部为了把object[]转换为对应的int[]，创建了新的数组 ----A
            if (_engine is JintEngine)
            // NLua中还是原引用
            Assert.That(arr[1], Is.EqualTo(2));
        }

        /// <summary>
        /// 脚本中调用C#方法
        /// </summary>
        public virtual void CallStaticMethod()
        {
            _engine.AddHostedType("ScriptTestStaticClass", typeof(ScriptTestStaticClass));
            _engine.Evaluate(Code);

            var test = _engine.CallFunction("test").ToObject<ScriptTestClass>();
            Assert.That(test.IntValue, Is.EqualTo(1));
        }

        /// <summary>
        /// 调用通过参数传入的实体并返回未注册的类型
        /// </summary>
        public virtual void CallMethodByParams()
        {
            _engine.Evaluate(Code);

            var manager = new ScriptTestClassManager();
            var obj = _engine.CallFunction("test", manager).ToObject<ScriptTestClass>();
            Assert.That(obj.IntValue, Is.EqualTo(1));
        }

        /// <summary>
        /// 脚本中使用C#中定义的枚举
        /// </summary>
        public virtual void UseEnumTest()
        {
            _engine.AddHostedType("ScriptTestEnmu", typeof(ScriptTestEnmu));
            _engine.Evaluate(Code);

            var test1 = _engine.CallFunction("test1").ToObject<ScriptTestEnmu>();
            Assert.That(test1, Is.EqualTo(ScriptTestEnmu.B));

            var test11 = _engine.CallFunction("test1").ToObject<int>();
            Assert.That(test11, Is.EqualTo(1));

            var test2 = _engine.CallFunction("test2").ToObject<ScriptTestEnmu>();
            Assert.That(test2, Is.EqualTo(ScriptTestEnmu.B));
        }

        /// <summary>
        /// 调用的C#方法存在可选参数
        /// </summary>
        public virtual void CheckOptional()
        {
            _engine.AddHostedType("ScriptTestStaticClass", typeof(ScriptTestStaticClass));
            _engine.Evaluate(Code);
            var d1 = _engine.CallFunction("test1", 1).ToObject<int>();
            Assert.That(d1, Is.EqualTo(1));

            var d2 = _engine.CallFunction("test2", 1, true).ToObject<int>();
            Assert.That(d2, Is.EqualTo(1));

            var d3 = _engine.CallFunction("test1", 0).ToObject<int>();
            Assert.That(d3, Is.EqualTo(1));

            var d4 = _engine.CallFunction("test1", true).ToObject<int>();
            Assert.That(d4, Is.EqualTo(2));
        }
        public virtual void CheckExsited()
        {
            _engine.Evaluate(Code);
            Assert.That(_engine.IsExisted("test_function"), Is.EqualTo(true));

            Assert.That(_engine.IsExisted("v1"), Is.EqualTo(false));
            Assert.That(_engine.IsExisted("v2"), Is.EqualTo(true));
        }


        public virtual void Script2CSharpArray()
        {
            _engine.AddHostedType("ScriptTestStaticClass", typeof(ScriptTestStaticClass));
            _engine.Evaluate(Code);
            Assert.That(_engine.CallFunction("test").ToObject<int>(), Is.EqualTo(3));
        }

        public void CheckFunctionReturnValue(string functionName, int value)
        {
            _engine.Evaluate(Code);
            Assert.That(_engine.CallFunction(functionName).ToObject<int>(), Is.EqualTo(value));
        }

        public virtual void TestChinese()
        {
            _engine.AddHostedType("Console", typeof(Console));
            _engine.AddHostedType("ScriptTestStaticClass", typeof(ScriptTestStaticClass));
            _engine.Evaluate(Code);


            var testString = "中文测试";
            Assert.That(_engine.CallFunction("test_chinese", testString).ToObject<string>(), Is.EqualTo(testString));
        }
    }
}
