namespace Application.Scripting
{
    public class ScriptMeta
    {
        public ScriptMeta(ScriptFile scriptFile, ScriptPrepareWrapper preparedValue, string fullPath)
        {
            ScriptFile = scriptFile;
            PreparedValue = preparedValue;
            FullPath = fullPath;
        }

        public ScriptFile ScriptFile { get; set; }
        public ScriptPrepareWrapper PreparedValue { get; set; }
        public string FullPath { get; set; }

        public override string ToString()
        {
            return FullPath;
        }
    }
}
