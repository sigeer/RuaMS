using Application.Shared.Items;

namespace Application.Shared.Models
{
    public record RefineFormula(int TargetItemId, List<ItemQuantity> Items, int Cost);
}
