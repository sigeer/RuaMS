using Application.Scripting.JS;
using System.Drawing;

namespace ServiceTest.Infrastructure.Scripts
{
    public class JintScriptEngineTest : BaseScriptTest
    {
        public JintScriptEngineTest() : base()
        {
            _engine = new JintEngine();
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _engine.Dispose();
        }

        [Test]
        public override void GetValueTest()
        {
            Code = """
                var p1 = 1;
                var p2 = "2";
                var p3 = "v3";
                var p4 = "1.4";
                var p5;
                """;
            base.GetValueTest();
        }

        [Test]
        public override void CallStaticMethod()
        {
            Code = """
                function test() {
                    return ScriptTestStaticClass.GetObject();
                }
                """;
            base.CallStaticMethod();
        }

        [Test]
        public override void CallMethodByParams()
        {
            Code = """
                function test(m) {
                    return m.GetValue(1);
                }
                """;
            base.CallMethodByParams();
        }

        [Test]
        public override void CheckExsited()
        {
            Code = """
                let v1;
                let v2 = 2;

                function test_function() {
                    return 1;
                }
                """;
            base.CheckExsited();
        }


        [Test]
        public void CheckExtension()
        {
            Code = """
                function test() {
                var p1 = new Point(0, 0);
                var p2 = new Point(3, 4);
                    return p1.distance(p2);
                }
                """;

            _engine.AddHostedType("Point", typeof(Point));
            _engine.Evaluate(Code);
            var d = _engine.CallFunction("test");
            Assert.That(d.ToObject<float>(), Is.EqualTo(5.0f));
        }

        [Test]
        public void CheckGetItemTest()
        {
            Code = """
                function test(list) {
                    return list.get(1);
                }
                """;

            _engine.Evaluate(Code);
            var d1 = _engine.CallFunction("test", new object[] { new List<string>() { "1", "2", "3" } });
            Assert.That(d1.ToObject<string>(), Is.EqualTo("2"));

            var d2 = _engine.CallFunction("test", new object[] { new string[] { "3", "4", "5" } });
            Assert.That(d2.ToObject<string>(), Is.EqualTo("4"));
        }

        [Test]
        public override void CheckMathRandomTest()
        {
            Code = """
            function test() {
                return Math.random();
            }
            """;
            base.CheckMathRandomTest();
        }

        [Test]
        public override void CheckOptional()
        {
            Code = """
                function test1(p1) {
                    return ScriptTestStaticClass.CheckOptional(p1);
                }

                function test2(p1, p2) {
                    return ScriptTestStaticClass.CheckOptional(p1, p2);
                }
                """;
            base.CheckOptional();
        }

        [Test]
        public void CheckSizeTest()
        {
            _engine.Evaluate("""
                function test(arr) {
                    return arr.Size();
                }
                """);

            int[] arr = [1, 2, 3, 4];
            var d = _engine.CallFunction("test", new object[] { arr }).ToObject<int>();
            Assert.That(d, Is.EqualTo(arr.Size()));

        }

        [Test]
        public override void ReturnScriptArray()
        {
            Code = """
                function test() {
                    return [1, 2, 3, 4, 6];
                }
                """;
            base.ReturnScriptArray();
        }

        [Test]
        public override void ReturnScriptBoolean()
        {
            Code = """
                function test_true() {
                    return true;
                }

                function test_false() {
                    return false;
                }

                function test() {}
                """;
            base.ReturnScriptBoolean();
        }

        public override void ReturnScriptDouble()
        {
            Code = """
                function test() {
                    return 1.234;
                }
                """;
            base.ReturnScriptDouble();
        }

        [Test]
        public override void ReturnScriptInt()
        {
            Code = """
                function test() {
                    return 1234.56;
                }
                """;
            base.ReturnScriptInt();
        }

        [Test]
        public override void ReturnScriptNewObject()
        {
            Code = """
                function test() {
                    var n = new ScriptTestClass();
                    n.ShortValue = 1;
                    return n;
                }
                """;
            base.ReturnScriptNewObject();
        }

        [Test]
        public override void ReturnScriptNewObjectArray()
        {
            Code = """
                function test() {
                    var arr = [];
                    arr.push(new ScriptTestClass());
                    arr.push(new ScriptTestClass());
                    arr.push(new ScriptTestClass());
                    return arr;
                }
                """;
            base.ReturnScriptNewObjectArray();
        }

        [Test]
        public override void UpdateArray()
        {
            Code = """
                function test(arr) {
                    arr[1] = 100;
                    return arr;
                }
                """;
            base.UpdateArray();
        }

        [Test]
        public override void UpdateObject()
        {
            Code = """
                function test(o) {
                    o.ShortValue = 999;
                    return o;
                }
                """;
            base.UpdateObject();
        }

        [Test]
        public override void UseEnumTest()
        {
            Code = """
                function test1() {
                    var m = ScriptTestEnmu.B;
                    return m;
                }

                function test2() {
                    return 1;
                }
                """;
            base.UseEnumTest();
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

                function test() {
                    return new Date().formatDate();
                }
                """);
            var d = _engine.CallFunction("test")?.ToString();
            Assert.That(d, Is.EqualTo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        [Test]
        public override void Script2CSharpArray()
        {
            Code = """
                function test() {
                    return ScriptTestStaticClass.PrintScriptArray([1,2,3]);
                }
                """;
            base.Script2CSharpArray();
        }

        //[Test]
        //public void NewList()
        //{
        //    Code = """
        //        function test() {
        //            return new List<int>();
        //        }
        //        """;
        //    base.TestFunctionReturn0();
        //}
    }
}
