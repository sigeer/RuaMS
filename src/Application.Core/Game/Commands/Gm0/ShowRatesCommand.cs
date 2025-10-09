using Application.Resources.Messages;
using System.Text;

namespace Application.Core.Game.Commands.Gm0;

public class ShowRatesCommand : CommandBase
{
    public ShowRatesCommand() : base(0, "showrates")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        StringBuilder sb = new StringBuilder();
        sb.Append("#e").Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_Exp))).Append("#n").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_WorldExp))).Append(": #k").Append(c.CurrentServer.WorldExpRate).Append("x#k").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerRawExp))).Append(": #k").Append(player.getRawExpRate()).Append("x#k").Append("\r\n");
        if (player.getCouponExpRate() != 1)
        {
            sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_CouponExp))).Append(": #k").Append(player.getCouponExpRate()).Append("x#k").Append("\r\n");
        }
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerExp))).Append(": #e#b").Append(player.getExpRate()).Append("x#k#n");
        if (player.hasNoviceExpRate())
            sb.Append(" - novice rate");
        sb.Append("\r\n\r\n");

        sb.Append("#e").Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_Meso))).Append("#n").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_WorldMeso))).Append(": #k").Append(c.CurrentServer.WorldMesoRate).Append("x#k").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerRawMeso))).Append(": #k").Append(player.getRawMesoRate()).Append("x#k").Append("\r\n");
        if (player.getCouponMesoRate() != 1)
        {
            sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_CouponMeso))).Append(": #k").Append(player.getCouponMesoRate()).Append("x#k").Append("\r\n");
        }
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerMeso))).Append(": #e#b").Append(player.getMesoRate()).Append("x#k#n");
        sb.Append("\r\n\r\n");

        sb.Append("#e").Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_Drop))).Append("#n").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_WorldDrop))).Append(": #k").Append(c.CurrentServer.WorldDropRate).Append("x#k").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerRawDrop))).Append(": #k").Append(player.getRawDropRate()).Append("x#k").Append("\r\n");
        if (player.getCouponDropRate() != 1)
        {
            sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_CouponDrop))).Append(": #k").Append(player.getCouponDropRate()).Append("x#k").Append("\r\n");
        }
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerDrop))).Append(": #e#b").Append(player.getDropRate()).Append("x#k#n");
        sb.Append("\r\n\r\n");

        sb.Append("#e").Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_BossDrop))).Append("#n").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_WorldBossDrop))).Append(": #k").Append(c.CurrentServer.WorldBossDropRate).Append("x#k").Append("\r\n");
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_PlayerBossDrop))).Append(": #e#b").Append(player.getBossDropRate()).Append("x#k#n");
        sb.Append("\r\n\r\n");

        if (YamlConfig.config.server.USE_QUEST_RATE)
        {
            sb.Append("#e").Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_Quest))).Append("#n").Append("\r\n");
            sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_WorldQuest))).Append(": #k").Append(c.CurrentServer.WorldQuestRate).Append("x#k").Append("\r\n");
        }

        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Rate_WorldTravel))).Append(": #e#b").Append(c.CurrentServer.WorldTravelRate).Append("x#k#n");
        sb.Append("\r\n");

        player.showHint(sb.ToString(), 300);
    }
}
