using Application.Core.Scripting.Infrastructure;
using System.Drawing;

namespace ServiceTest.Infrastructure
{
    public class ScriptingTests
    {
        IEngine _engine;
        ScriptTestCase _testModel;
        public ScriptingTests()
        {
            _engine = new JintEngine();
            _testModel = new ScriptTestCase(_engine);
        }

        [Test]
        public void CheckMathRandomTest()
        {
            _engine.Evaluate("""
                function check_math() {
                    return Math.random();
                }
                """);
            var d = _engine.CallFunction("check_math");
            Assert.That(d is double v && v < 1);
        }

        [Test]
        public void CheckSizeTest()
        {
            _engine.Evaluate("""
                function check_size(list) {
                    return list.Size();
                }
                """);
            var d = _engine.CallFunction("check_size", new object[] { new List<string>() { "1", "2", "3" } });
            var id = Convert.ToInt32(d);
            Assert.That(Is.Equals(id, 3));
        }

        [Test]
        public void CheckGetTest()
        {
            _engine.Evaluate("""
                function check_get(list) {
                    return list.get(1);
                }
                """);
            var d = _engine.CallFunction("check_get", new object[] { new List<string>() { "1", "2", "3" } });
            Assert.That(Is.Equals(d, "2"));
        }

        [Test]
        public void CallCsharpWithArrayTest()
        {
            _engine.Evaluate("""
                function call_csharp_with_array(m) {
                    var arr = [1122004, 1012078, 1432008];
                    return m.FromJs(arr);
                }
                """);
            var d = _engine.CallFunction("call_csharp_with_array", [_testModel]);
            var id = Convert.ToInt32(d);
            Assert.That(Is.Equals(id, 3));
        }
        [Test]
        public void CallCsharpUpdateJsValueTest()
        {
            _engine.Evaluate("""
                var i = 1;
                function call_csharp_update_jsvalue(m) {
                    m.UpdateJsValue(i);
                    return i;
                }
                """);
            var d = _engine.CallFunction("call_csharp_update_jsvalue", [_testModel]);
            var id = Convert.ToInt32(d);
            Assert.That(Is.Equals(id, 2));
        }

        [Test]
        public void UseEnumTest()
        {
            _engine.Evaluate("""
                function use_enum(m) {
                    return m.UseEnum(E.B);
                }
                """);
            _engine.AddHostedType("E", typeof(TestEnum));
            var d = _engine.CallFunction("use_enum", [_testModel]);
            Assert.That(Is.Equals(d, "B"));
        }
        [Test]
        public void CheckReturnIntTest()
        {
            _engine.Evaluate("""
                function check_return_int() {
                    return 123;
                }
                """);
            var d = Convert.ToInt32(_engine.CallFunction("check_return_int"));
            Assert.That(Is.Equals(d, 123));
        }
        [Test]
        public void CheckReturnDoubleTest()
        {
            _engine.Evaluate("""
                function check_return_double() {
                    return 123.456;
                }
                """);
            var d = Convert.ToDouble(_engine.CallFunction("check_return_double"));
            Assert.That(Is.Equals(d, 123.456));
        }
        [Test]
        public void CheckReturnBoolTest()
        {
            _engine.Evaluate("""
                function check_return_bool() {
                    return true;
                }
                """);
            var d = Convert.ToBoolean(_engine.CallFunction("check_return_bool"));
            Assert.That(Is.Equals(d, true));
        }

        [Test]
        public void CheckReturnObjectTest()
        {
            _engine.Evaluate("""
                function check_return_object() {
                    return new Point(4,5);
                }
                """);
            _engine.AddHostedType("Point", typeof(Point));
            var d = (Point)_engine.CallFunction("check_return_object");
            Assert.That(Is.Equals(d.X, 4));
            Assert.That(Is.Equals(d.Y, 5));
        }

        [Test]
        public void CheckReturnArrayTest()
        {
            _engine.Evaluate("""
                function check_return_array() {
                    var arr = [];
                    arr.push(new Point(1, 1));
                    arr.push(new Point(2, 2));
                    arr.push(new Point(3, 3));
                    return arr;
                }
                """);
            _engine.AddHostedType("Point", typeof(Point));
            var d = (_engine.CallFunction("check_return_array") as object[]).OfType<Point>().ToList();
            Assert.That(d.Count == 3);
        }
    }

    public class ScriptTestCase
    {
        readonly IEngine engine;
        public ScriptTestCase(IEngine engine)
        {
            this.engine = engine;
        }

        public int FromJs(List<object> list)
        {
            return list.Count;
        }

        public void UpdateJsValue(int i)
        {
            engine.Evaluate("i++");
        }

        public string UseEnum(TestEnum d)
        {
            return d.ToString();
        }
    }

    public enum TestEnum
    {
        A, B, C, D
    }
}
