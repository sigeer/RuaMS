namespace ServiceTest.Infrastructure.Scripts
{
    public class ScriptTestClassManager
    {
        public Dictionary<int, ScriptTestClass> DataSource = new Dictionary<int, ScriptTestClass>() { { 1, new ScriptTestClass { IntValue = 1 } } };

        public ScriptTestClass? GetValue(int i)
        {
            return DataSource.GetValueOrDefault(i);
        }
    }

    public class ScriptTestClass
    {
        public int IntValue { get; set; }
        public short ShortValue { get; set; }
        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
    }

    public enum ScriptTestEnmu
    {
        A,B,C
    }

    public class ScriptTestStaticClass
    {
        public static ScriptTestClass GetObject()
        {
            return new ScriptTestClass() { IntValue = 1};
        }

        public static int CheckOptional(short s, bool b = true)
        {
            return 1;
        }

        public static int CheckOptional(bool b)
        {
            return 2;
        }

        public static int PrintScriptArray(int[] list)
        {
            return list.Length;
        }

        public static int PrintObjectListCount(List<object> list)
        {
            return list.Count;
        }

        public static string TestChinese(string str)
        {
            Console.WriteLine("C#: " + str);
            return str;
        }


        public static string ParamsFunction(string input, params string[] paramsValue)
        {
            return string.Format(input, paramsValue);
        }
    }
}
