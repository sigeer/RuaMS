namespace client.inventory;

public record ItemQuantity(int ItemId, int Quantity);

public record TypedItemQuantity(int Type, ItemQuantity Item);

public record StatuedTypedItemQuantity(int status, List<TypedItemQuantity> data);