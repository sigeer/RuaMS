

using client;
using net.packet;
using scripting.quest;
using server.quest;
using static Application.Core.Game.Players.Player;

namespace net.server.channel.handlers;

/**
 * @author Xari
 */
public class RaiseUIStateHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        int infoNumber = p.readShort();

        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;
                Quest quest = Quest.getInstanceFromInfoNumber(infoNumber);
                QuestStatus mqs = chr.getQuest(quest);

                QuestScriptManager.getInstance().raiseOpen(c, (short)infoNumber, mqs.getNpc());

                if (mqs.getStatus() == QuestStatus.Status.NOT_STARTED)
                {
                    quest.forceStart(chr, 22000);
                    c.getAbstractPlayerInteraction().setQuestProgress(quest.getId(), infoNumber, 0);
                }
                else if (mqs.getStatus() == QuestStatus.Status.STARTED)
                {
                    chr.announceUpdateQuest(DelayedQuestUpdate.UPDATE, mqs, mqs.getInfoNumber() > 0);
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}