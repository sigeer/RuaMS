namespace Application.Core.model
{
    public record RewardPools(ItemQuantity[] ItemPool, int[] MesoPool, int[] ExpPool);

    public record RewardOptions(
        float FinalExpRate = 1,
        float FinalMesoRate = 1,
        int ExpPoolIndex = -1,
        int MesoPoolIndex = -1);
}
