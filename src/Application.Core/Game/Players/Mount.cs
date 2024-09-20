using Application.Core.Game.Items;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public IMount? MountModel { get; private set; }

        public void SetMount(IMount? mount)
        {
            MountModel = mount;
        }

        public IMount? getMount()
        {
            return MountModel;
        }
        public IMount mount(int id, int skillid)
        {
            var mount = MountModel!;
            mount.setItemId(id);
            mount.setSkillId(skillid);
            return mount;
        }

        public bool runTirednessSchedule()
        {
            if (MountModel != null)
            {
                int tiredness = MountModel.incrementAndGetTiredness();

                this.MapModel.broadcastMessage(PacketCreator.updateMount(this.getId(), MountModel, false));
                if (tiredness > 99)
                {
                    MountModel.setTiredness(99);
                    this.dispelSkill(this.getJobType() * 10000000 + 1004);
                    this.dropMessage(6, "Your mount grew tired! Treat it some revitalizer before riding it again!");
                    return false;
                }
            }

            return true;
        }
    }
}
