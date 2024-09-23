using client.inventory;

namespace ServiceTest.Items
{
    public class ItemFactoryTests: TestBase
    {
        [Test]
        public void loadEquippedItems_Test()
        {
            var data = ItemFactory.loadEquippedItems(1);
            Assert.That(data.Count > 0);
        }
    }
}
