namespace Application.Scripting
{
    public interface IAddtionalRegistry
    {
        public void AddHostedObject(IEngine engine);
        public void AddHostedType(IEngine engine);
    }
}
