using Application.Core.Channel.DataProviders;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;

namespace Application.Core.Game.Commands.Gm1;
public class WhatDropsFromCommand : CommandBase
{
    public WhatDropsFromCommand() : base(1, "whatdropsfrom")
    {
        Description = "Show what items drop from a mob.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, "Please do @whatdropsfrom <monster name>");
            return;
        }
        string monsterName = player.getLastCommandMessage();
        string output = "";
        int limit = 3;
        var dataList = ProviderFactory.GetProvider<StringProvider>().Search(StringCategory.Mob, monsterName, limit).OfType<StringTemplate>();
        foreach (var data in dataList)
        {
            int mobId = data.TemplateId;
            string mobName = data.Name;
            output += mobName + " drops the following items:\r\n\r\n";
            foreach (var drop in MonsterInformationProvider.getInstance().retrieveDrop(mobId))
            {
                try
                {
                    if (drop.Chance == 0 || !ItemInformationProvider.getInstance().HasTemplate(drop.ItemId))
                    {
                        continue;
                    }
                    float chance = Math.Max(1000000 / drop.Chance / (!MonsterInformationProvider.getInstance().isBoss(mobId) ? player.getDropRate() : player.getBossDropRate()), 1);
                    output += "- " + ItemInformationProvider.getInstance().getName(drop.ItemId) + " (1/" + (int)chance + ")\r\n";
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                    continue;
                }
            }
            output += "\r\n";

        }

        c.getAbstractPlayerInteraction().npcTalk(NpcId.MAPLE_ADMINISTRATOR, output);
    }
}
