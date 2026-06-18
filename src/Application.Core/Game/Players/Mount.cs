using Application.Core.Game.Items;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public Mount? MountModel { get; private set; }

        public void SetMount(Mount? mount)
        {
            MountModel = mount;
        }

        public Mount? getMount()
        {
            return MountModel;
        }
        public Mount mount(int id, int skillid)
        {
            var mount = MountModel!;
            mount.setItemId(id);
            mount.setSkillId(skillid);
            mount.setActive(true);
            return mount;
        }
    }
}
