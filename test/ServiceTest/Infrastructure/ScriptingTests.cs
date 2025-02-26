using Application.Core.Scripting.Infrastructure;
using Jint;
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
            Assert.That(id, Is.EqualTo(3));
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
            Assert.That(d, Is.EqualTo("2"));
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
            Assert.That(id, Is.EqualTo(3));
        }

        [Test]
        public void CallCsharpWithIntTest()
        {
            _engine.Evaluate("""
                function call_csharp_with_int(m) {
                    var arr = 123;
                    return m.FromJs(arr);
                }
                """);
            var d = _engine.CallFunction("call_csharp_with_int", [_testModel]);
            var id = Convert.ToInt32(d);
            Assert.That(id, Is.EqualTo(123));
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
            Assert.That(id, Is.EqualTo(2));
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
            Assert.That(d, Is.EqualTo("B"));
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
            Assert.That(d, Is.EqualTo(123));
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
            Assert.That(d, Is.EqualTo(123.456));
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
            Assert.That(d, Is.EqualTo(true));
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
            Assert.That(d.X, Is.EqualTo(4));
            Assert.That(d.Y, Is.EqualTo(5));
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
            Assert.That(d.Count, Is.EqualTo(3));
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _engine.Dispose();
        }

        [Test]
        public void CheckOptional()
        {
            _engine.Evaluate("""
                function CheckNumberBool(p) {
                    return p.Method1(1, 100);
                }
                """);
            var d = _engine.CallFunction("CheckNumberBool", _testModel)?.ToString();
            Assert.That(d, Is.EqualTo("1")); 
        }

        [Test]
        public void CheckExsited()
        {
            _engine.Evaluate("""
                let v1;
                let v2 = 2;
                var v3;
                var v4 = 4;
                function Method1() {
                    return 1;
                }
                """);
            Assert.That(_engine.IsExisted("Method1"), Is.EqualTo(true));

            Assert.That(_engine.IsExisted("v1"), Is.EqualTo(false));
            Assert.That(_engine.IsExisted("v2"), Is.EqualTo(true));

            Assert.That(_engine.IsExisted("v3"), Is.EqualTo(false));
            Assert.That(_engine.IsExisted("v4"), Is.EqualTo(true));

            Assert.That(_engine.IsFunctionExisted("v2"), Is.EqualTo(false));
            Assert.That(_engine.IsFunctionExisted("v4"), Is.EqualTo(false));
            Assert.That(_engine.IsFunctionExisted("Method1"), Is.EqualTo(true));
        }


        [Test]
        public void PrototypeTest()
        {
            _engine.Evaluate("""
                Date.prototype.formatDate = function () {
                    var year = this.getFullYear();
                    var month = (this.getMonth() + 1).toString().padStart(2, '0');
                    var day = (this.getDate()).toString().padStart(2, '0');
                    var hour = (this.getHours()).toString().padStart(2, '0');
                    var minute = (this.getMinutes()).toString().padStart(2, '0');
                    var second = (this.getSeconds()).toString().padStart(2, '0');

                    return `${year}-${month}-${day} ${hour}:${minute}:${second}`;
                };

                function getDt() {
                    return new Date().formatDate();
                }
                """);
            var d = _engine.CallFunction("getDt")?.ToString();
            Assert.That(d, Is.EqualTo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
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

        public int FromJs(int i)
        {
            return i;
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

    public enum TestEnum
    {
        A, B, C, D
    }
}
