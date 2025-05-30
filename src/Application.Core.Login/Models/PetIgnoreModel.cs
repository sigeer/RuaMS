namespace Application.Core.Login.Models
{
    public class PetIgnoreModel
    {
        public long PetId { get; set; }
        public int[] ExcludedItems { get; set; }
    }
}
