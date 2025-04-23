using Application.Scripting.JS;
using Application.Scripting.Lua;
using Application.Utility.Extensions;
using System.Drawing;

namespace ServiceTest.Infrastructure.Scripts
{
    public class NLuaScriptEngineTest : BaseScriptTest
    {
        public NLuaScriptEngineTest()
        {
            _engine = new NLuaScriptEngine();
        }


        [OneTimeTearDown]
        public void Dispose()
        {
            _engine.Dispose();
        }

        [Test]
        public override void CallStaticMethod()
        {
            Code = """
                function test()
                    return ScriptTestStaticClass.GetObject()
                end
                """;
            base.CallStaticMethod();
        }

        [Test]
        public override void CallMethodByParams()
        {
            Code = """
                function test(m)
                    return m:GetValue(1)
                end
                """;
            base.CallMethodByParams();
        }

        [Test]
        public override void CheckExsited()
        {
            Code = """
                local v1
                v2 = 2

                function test_function()
                    return 1
                end
                """;
            base.CheckExsited();
        }

        [Test]
        public void CheckExtension()
        {
            Code = """
                function test()
                    local p1 = Point(0, 0);
                    local p2 = Point(3, 4);
                    return p1:distance(p2);
                end
                """;

            _engine.AddHostedType("PointExtensions", typeof(PointExtensions));
            _engine.AddHostedType("Point", typeof(Point));
            _engine.Evaluate(Code);
            var d = _engine.CallFunction("test");
            Assert.That(d.ToObject<float>(), Is.EqualTo(5.0f));
        }

        [Test]
        public void CheckGetItemTest()
        {
            Code = """
                function test(list)
                    return list[1]
                end
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
            function test()
                return math.random()
            end
            """;
            base.CheckMathRandomTest();
        }

        [Test]
        public override void CheckOptional()
        {
            Code = """
                function test1(p1)
                    return ScriptTestStaticClass.CheckOptional(p1)
                end

                function test2(p1, p2)
                    return ScriptTestStaticClass.CheckOptional(p1, p2)
                end
                """;
            base.CheckOptional();
        }

        [Test]
        public void CheckSizeTest()
        {
            _engine.Evaluate("""
                function test(arr)
                    return arr.Length
                end
                """);

            int[] arr = [1, 2, 3, 4];
            var d = _engine.CallFunction("test", new object[] { arr }).ToObject<int>();
            Assert.That(d, Is.EqualTo(arr.Size()));

        }

        [Test]
        public override void ReturnScriptArray()
        {
            Code = """
                function test()
                    return {1, 2, 3, 4, 6}
                end
                """;
            base.ReturnScriptArray();
        }

        [Test]
        public override void ReturnScriptBoolean()
        {
            Code = """
                function test_true()
                    return true
                end

                function test_false()
                    return false
                end

                function test()
                end
                """;
            base.ReturnScriptBoolean();
        }

        public override void ReturnScriptDouble()
        {
            Code = """
                function test()
                    return 1.234
                end
                """;
            base.ReturnScriptDouble();
        }

        [Test]
        public override void ReturnScriptInt()
        {
            Code = """
                function test()
                    return 1234.56;
                end
                """;
            base.ReturnScriptInt();
        }

        [Test]
        public override void ReturnScriptNewObject()
        {
            Code = """
                function test()
                    n = ScriptTestClass();
                    n.ShortValue = 1
                    return n;
                end
                """;
            base.ReturnScriptNewObject();
        }

        [Test]
        public override void ReturnScriptNewObjectArray()
        {
            Code = """
                function check_return()
                    local arr = {};
                    table.insert(arr, ScriptTestClass())
                    table.insert(arr, ScriptTestClass())
                    table.insert(arr, ScriptTestClass())
                    return arr;
                end

                function check_params()
                    local arr = {};
                    table.insert(arr, ScriptTestClass())
                    table.insert(arr, ScriptTestClass())
                    table.insert(arr, ScriptTestClass())

                    return ScriptTestStaticClass.PrintObjectListCount(LuaTableUtils.ToListA(arr, "ServiceTest.Infrastructure.Scripts.ScriptTestClass"));
                end
                """;
            base.ReturnScriptNewObjectArray();
        }

        [Test]
        public override void UpdateArray()
        {
            Code = """
                function test(arr)
                    arr[1] = 100
                    return arr
                end
                """;
            base.UpdateArray();
        }

        [Test]
        public override void UpdateObject()
        {
            Code = """
                function test(o)
                    o.ShortValue = 999
                    return o
                end
                """;
            base.UpdateObject();
        }

        [Test]
        public override void UseEnumTest()
        {
            Code = """
                function test1()
                    m = ScriptTestEnmu.B
                    return m
                end

                function test2()
                    return 1
                end
                """;
            base.UseEnumTest();
        }

        [Test]
        public override void Script2CSharpArray()
        {
            // nlua 不能配置
            Code = """
                function test()
                    local arr = {1, 2, 3}
                    return ScriptTestStaticClass.PrintScriptArray(LuaTableUtils.ToInt32Array({2, 3, 4}))
                end
                """;

            base.Script2CSharpArray();
        }

        [Test]
        public void NewList()
        {
            Code = """
                function test()
                    local List = luanet.import_type("System.Collections.Generic.List`1[System.Int32]")
                    local list = List()
                    return list.Count
                end
                """;
            base.CheckFunctionReturnValue("test", 0);
        }

        [Test]
        public void NewArray()
        {
            Code = """
                function test()
                local Int32 = luanet.import_type("System.Int32")
                local arr = Int32[0]
                return arr.Length
                end
                """;
            base.CheckFunctionReturnValue("test", 0);
        }
    }
}
