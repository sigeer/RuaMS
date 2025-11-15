namespace Application.Templates
{
    /// <summary>
    /// 用于指示字段在img中的路径，辅助SourceGenerator。
    /// <para>一些特殊的词： "-"表示数组  "$name" 表示数组索引 "$length" 子节点数量 "~"相对路径</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class WZPathAttribute : Attribute
    {
        public WZPathAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
