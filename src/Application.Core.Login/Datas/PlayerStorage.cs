using Application.Shared.Dto;

namespace Application.Core.Login.Datas
{
    public class PlayerStorage
    {
        Dictionary<int, CharacterValueObject> _idDataSource = new Dictionary<int, CharacterValueObject>();
        Dictionary<string, CharacterValueObject> _nameDataSource = new Dictionary<string, CharacterValueObject>();

        public int Count => _idDataSource.Count;
    }
}
