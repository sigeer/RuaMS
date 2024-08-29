using client;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public Mount? MountModel { get; set; }

        public Mount? getMount()
        {
            return MountModel;
        }
        public Mount mount(int id, int skillid)
        {
            Mount mount = MountModel!;
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
