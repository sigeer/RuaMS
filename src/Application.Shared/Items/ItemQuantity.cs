namespace Application.Shared.Items;

public record ItemQuantity(int ItemId, int Quantity);

public record TypedItemQuantity(int Type, ItemQuantity Item);