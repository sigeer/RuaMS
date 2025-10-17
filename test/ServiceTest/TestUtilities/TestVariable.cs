namespace ServiceTest.TestUtilities
{
    public class TestVariable
    {
        public static readonly string WzPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "Application.Resources", "wz"));
    }
}
