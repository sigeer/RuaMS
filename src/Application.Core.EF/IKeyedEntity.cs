namespace Application.Core.EF
{
    public interface IKeyedEntity<TKey> where TKey : notnull
    {
        TKey Id { get; set; }
    }
}
