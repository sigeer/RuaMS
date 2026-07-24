namespace Application.Templates.Etc
{
    [GenerateTag]
    public class ItemMakeTemplate : AbstractTemplate
    {
        public ItemMakeTemplate(int templateId) : base(templateId)
        {
            Rewards = [];
            Recipes = [];
        }

        [WZPath("reqLevel")]
        public int ReqLevel { get; set; }
        public int ReqSkillLevel { get; set; }
        public int ItemNum { get; set; }
        public int Tuc { get; set; }
        public int Meso { get; set; }

        public int Catalyst { get; set; } = -1;
        [WZPath("randomReward/-")]
        public ItemMakeReward[] Rewards { get; set; }
        [WZPath("recipe/-")]
        public ItemMakeRecipe[] Recipes { get; set; }
    }

    public class ItemMakeReward
    {
        public int Item { get; set; }
        public int ItemNum { get; set; }
        public int Prob { get; set; }
    }

    public class ItemMakeRecipe
    {
        public int Item { get; set; }
        public int Count { get; set; }
    }
}
