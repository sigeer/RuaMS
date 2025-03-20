using Application.Scripting;
using Application.Scripting.Lua;
using MoonSharp.Interpreter;
using System.Drawing;

namespace ServiceTest.Infrastructure
{
    public class LuaScriptingTests
    {
        LuaScriptEngine _engine;
        LuaScriptTestCase _testModel;
        public LuaScriptingTests()
        {
            _engine = new LuaScriptEngine();
            _testModel = new LuaScriptTestCase(_engine);
        }

        [Test]
        public void CheckMathRandomTest()
        {
            _engine.Evaluate("""
                function check_math()
                    return math.random()
                end
                """);
            var d = _engine.CallFunction("check_math");
            Assert.That(d?.ToObject<double>() < 1);
        }

        [Test]
        public void FunctionParamTest()
        {
            _engine.Evaluate("""
                function function_param_test(m)
                    return m
                end
                """);

            var d = _engine.CallFunction("function_param_test", _testModel);
            Assert.That(d?.ToObject<LuaScriptTestCase>() != null);
        }

        [Test]
        public void UseEnumTest()
        {
            _engine.AddHostedType("E", typeof(TestEnum));
            _engine.Evaluate("""
                function use_enum(m)
                    return m.UseEnum(E.B)
                end
                """);

            var d = _engine.CallFunction("use_enum", [_testModel]);
            Assert.That(d?.ToObject<string>(), Is.EqualTo("B"));
        }
        [Test]
        public void CheckReturnIntTest()
        {
            _engine.Evaluate("""
                function check_return_int()
                    return 123
                end
                """);
            var d = _engine.CallFunction("check_return_int");
            Assert.That(d?.ToObject<int>(), Is.EqualTo(123));
        }

        [Test]
        public void CheckReturnArrayTest()
        {
            _engine.Evaluate("""
                function test_fun()
                    return {1,2,3,4}
                end
                """);
            var d = _engine.CallFunction("test_fun").ToObject<List<int>>();
            Assert.That(d.Count == 4);
        }

        [Test]
        public void CheckReturnArrayFromScriptTest()
        {
            _engine.Evaluate("""
                function test_fun(m)
                    return m.FromTable({1,2,3})
                end
                """);
            var d = _engine.CallFunction("test_fun", new LuaScriptTestCase(_engine)).ToObject<int>();
            Assert.That(d == 3);
        }
        [Test]
        public void CheckReturnDoubleTest()
        {
            _engine.Evaluate("""
                function check_return_double()
                    return 123.456
                end
                """);
            var d = _engine.CallFunction("check_return_double");
            Assert.That(d.ToObject<double>(), Is.EqualTo(123.456));
        }
        [Test]
        public void CheckReturnBoolTest()
        {
            _engine.Evaluate("""
                function check_return_bool()
                    return true
                end
                """);
            var d = _engine.CallFunction("check_return_bool");
            Assert.That(d.ToObject<bool>(), Is.EqualTo(true));
        }

        [Test]
        public void CheckReturnPointTest()
        {
            _engine.RegisterPoint();
            _engine.Evaluate("""
                function check_new_point()
                    return Point(1, 2)
                end
                """);
            var d = _engine.CallFunction("check_new_point");
            Assert.DoesNotThrow(() => d.ToObject<Point>());
        }

        [Test]
        public void CheckArrayLengthTest()
        {
            // _engine.AddHostedType("Point", typeof(Point));
            _engine.Evaluate("""
                function test_fun(m)
                    return #m;
                end
                """);
            var array = new int[] { 1, 2, 3, 4 };
            var d = _engine.CallFunction("test_fun", new object[] { array });
            Assert.That(d.ToObject<int>() == 4);
        }

        [Test]
        public void CheckReturnSubObjectTest()
        {
            _engine.AddHostedType("LuaTestSubClass", typeof(LuaTestSubClass));
            _engine.Evaluate("""
                function test_fun(xy)
                    return xy.Sub;
                end
                """);
            var d = _engine.CallFunction("test_fun", new LuaTestClass());
            Assert.That(d.ToObject<LuaTestSubClass>() != null);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _engine.Dispose();
        }

        [Test]
        public void CheckExsited()
        {
            _engine.Evaluate("""
                local v1
                v2 = 2
                local v3
                v4 = 4
                function Method1()
                    return 1
                end
                """);
            Assert.That(_engine.IsExisted("Method1"), Is.EqualTo(true));

            Assert.That(_engine.IsExisted("v1"), Is.EqualTo(false));
            Assert.That(_engine.IsExisted("v2"), Is.EqualTo(true));

            Assert.That(_engine.IsExisted("v3"), Is.EqualTo(false));
            Assert.That(_engine.IsExisted("v4"), Is.EqualTo(true));
        }
    }

    public class LuaTestClass
    {
        public LuaTestSubClass Sub { get; set; } = new LuaTestSubClass();
    }
    public class LuaTestSubClass { }

    public class LuaScriptTestCase
    {
        readonly IEngine engine;
        public LuaScriptTestCase(IEngine engine)
        {
            this.engine = engine;
        }

        public int FromJs(List<object> list)
        {
            return list.Count;
        }

        public int FromJs(int i)
        {
            return i;
        }

        public int FromTable(object i)
        {
            return (i as Table).Pairs.Count();
        }

        public void UpdateJsValue(int i)
        {
            engine.Evaluate("i++");
        }

        public string UseEnum(TestEnum d)
        {
            return d.ToString();
        }

        public string Method1(int i, short s, bool b = true)
        {
            return "1";
        }

        public string Method1(int i, bool b)
        {
            return "2";
        }


    }

}
