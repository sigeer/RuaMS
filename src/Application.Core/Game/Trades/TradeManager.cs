using Application.Core.Game.Invites;
using Application.Shared.Invitations;
using client.inventory;
using server;
using System.Text;
using tools;

namespace Application.Core.Game.Trades
{
    public class TradeManager
    {
        private static ILogger log = LogFactory.GetLogger(LogType.Trade);

        public static int GetFee(long meso)
        {
            long fee = 0;
            if (meso >= 10000_0000)
            {
                fee = meso * 6 / 100;
            }
            else if (meso >= 2500_0000)
            {
                fee = meso * 5 / 100;
            }
            else if (meso >= 1000_0000)
            {
                fee = meso * 4 / 100;
            }
            else if (meso >= 500_0000)
            {
                fee = meso * 3 / 100;
            }
            else if (meso >= 100_0000)
            {
                fee = meso * 18 / 1000;
            }
            else if (meso >= 10_0000)
            {
                fee = meso * 8 / 1000;
            }
            return (int)fee;
        }

        public static void CancelTrade(IPlayer chr, TradeResult result)
        {
            chr.getTrade()?.CancelTrade(result);
        }

        public static bool StartTrade(IPlayer chr)
        {
            if (chr.getTrade() == null)
            {
                chr.setTrade(new Trade(0, chr));
                return true;
            }
            return false;
        }

        private static bool hasTradeInviteBack(IPlayer c1, IPlayer c2)
        {
            var other = c2.getTrade();
            if (other != null)
            {
                var otherPartner = other.PartnerTrade;
                if (otherPartner != null)
                {
                    return otherPartner.getChr().getId() == c1.getId();
                }
            }

            return false;
        }

        public static void InviteTrade(IPlayer c1, IPlayer c2)
        {

            if (c1.isGM() && !c2.isGM() && c1.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_TRADE)
            {
                c1.message("You cannot trade with non-GM characters.");
                log.Information("GM {GMName} blocked from trading with {CharacterName} due to GM level.", c1.getName(), c2.getName());
                CancelTrade(c1, TradeResult.NO_RESPONSE);
                return;
            }

            if (!c1.isGM() && c2.isGM() && c2.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_TRADE)
            {
                c1.message("You cannot trade with this GM character.");
                CancelTrade(c1, TradeResult.NO_RESPONSE);
                return;
            }

            if (InviteType.TRADE.HasRequest(c1.getId()))
            {
                if (hasTradeInviteBack(c1, c2))
                {
                    c1.message("You are already managing this player's trade invitation.");
                }
                else
                {
                    c1.message("You are already managing someone's trade invitation.");
                }

                return;
            }
            else if (c1.getTrade()!.isFullTrade())
            {
                c1.message("You are already in a trade.");
                return;
            }

            if (InviteType.TRADE.CreateInvite(new InviteRequest(c1, c2)))
            {
                if (JoinTrade(c1.getTrade()!, c2))
                {
                    c1.sendPacket(PacketCreator.getTradeStart(c1.getClient(), c1.getTrade()!, 0));
                    c2.sendPacket(PacketCreator.tradeInvite(c1));
                }
                else
                {
                    c1.message("The other player is already trading with someone else.");
                    CancelTrade(c1, TradeResult.NO_RESPONSE);
                    InviteType.TRADE.AnswerInvite(c2.getId(), c1.getId(), false);
                }
            }
            else
            {
                c1.message("The other player is already managing someone else's trade invitation.");
                CancelTrade(c1, TradeResult.NO_RESPONSE);
            }
        }

        public static bool JoinTrade(Trade trade, IPlayer player)
        {
            if (player.getTrade() == null)
            {
                var c2Trade = new Trade(1, player);
                player.setTrade(c2Trade);

                c2Trade.setPartner(trade);
                trade.setPartner(c2Trade);

                return true;
            }
            return false;
        }

        public static void VisitTrade(IPlayer c1, IPlayer c2)
        {
            InviteResult inviteRes = InviteType.TRADE.AnswerInvite(c1.getId(), c2.getId(), true);

            InviteResultType res = inviteRes.Result;
            if (res == InviteResultType.ACCEPTED)
            {
                if (c1.getTrade() != null && c1.getTrade()!.PartnerTrade == c2.getTrade() && c2.getTrade() != null && c2.getTrade()!.PartnerTrade == c1.getTrade())
                {
                    c2.sendPacket(PacketCreator.getTradePartnerAdd(c1));
                    c1.sendPacket(PacketCreator.getTradeStart(c1.getClient(), c1.getTrade()!, 1));
                    c1.getTrade()!.setFullTrade(true);
                    c2.getTrade()!.setFullTrade(true);
                }
                else
                {
                    c1.message("The other player has already closed the trade.");
                }
            }
            else
            {
                c1.message("This trade invitation already rescinded.");
                CancelTrade(c1, TradeResult.NO_RESPONSE);
            }
        }

        public static void DeclineTrade(IPlayer chr)
        {
            var trade = chr.getTrade();
            if (trade != null)
            {
                if (trade.PartnerTrade != null)
                {
                    IPlayer other = trade.PartnerTrade.getChr();
                    if (InviteType.TRADE.AnswerInvite(chr.Id, other.Id, false).Result == InviteResultType.DENIED)
                    {
                        other.message(chr.getName() + " has declined your trade request.");
                    }

                    other.getTrade()!.GainItemByCancel((byte)TradeResult.PARTNER_CANCEL);
                    other.setTrade(null);

                }
                trade.GainItemByCancel((byte)TradeResult.NO_RESPONSE);
                chr.setTrade(null);
            }
        }
        public static void logTrade(Trade trade1, Trade trade2)
        {
            string name1 = trade1.getChr().getName();
            string name2 = trade2.getChr().getName();
            StringBuilder message = new StringBuilder();
            message.Append(string.Format("Committing trade between {0} and {1}", name1, name2));
            //Trade 1 to trade 2
            message.Append(string.Format("Trading {0} -> {1}: {2} mesos, items: {3}", name1, name2,
                    trade1.Meso, getFormattedItemLogMessage(trade1.ItemList)));

            //Trade 2 to trade 1
            message.Append(string.Format("Trading {0} -> {1}: {2} mesos, items: {3}", name2, name1,
                    trade2.Meso, getFormattedItemLogMessage(trade2.ItemList)));

            log.Information(message.ToString());
        }

        private static string getFormattedItemLogMessage(List<Item> items)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            return string.Join(", ", items.Select(x => $"[{x.getQuantity()} {ii.getName(x.getItemId())} ({x.getItemId()})]"));
        }

        public static void CompleteTrade(IPlayer chr)
        {
            var local = chr.getTrade();
            if (local == null || local.PartnerTrade == null)
                return;

            if (local.checkCompleteHandshake())
            {
                var partner = local.PartnerTrade;
                if (!local.fitsMeso())
                {
                    local.CancelTrade(TradeResult.UNSUCCESSFUL);
                    chr.message("There is not enough meso inventory space to complete the trade.");
                    partner.getChr().message("Partner does not have enough meso inventory space to complete the trade.");
                    return;
                }
                else if (!partner.fitsMeso())
                {
                    partner.CancelTrade(TradeResult.UNSUCCESSFUL);
                    chr.message("Partner does not have enough meso inventory space to complete the trade.");
                    partner.getChr().message("There is not enough meso inventory space to complete the trade.");
                    return;
                }

                if (!local.fitsInInventory())
                {
                    if (local.fitsUniquesInInventory())
                    {
                        local.CancelTrade(TradeResult.UNSUCCESSFUL);
                        chr.message("There is not enough inventory space to complete the trade.");
                        partner.getChr().message("Partner does not have enough inventory space to complete the trade.");
                    }
                    else
                    {
                        local.CancelTrade(TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT);
                        partner.getChr().message("Partner cannot hold more than one one-of-a-kind item at a time.");
                    }
                    return;
                }
                else if (!partner.fitsInInventory())
                {
                    if (partner.fitsUniquesInInventory())
                    {
                        partner.CancelTrade(TradeResult.UNSUCCESSFUL);
                        chr.message("Partner does not have enough inventory space to complete the trade.");
                        partner.getChr().message("There is not enough inventory space to complete the trade.");
                    }
                    else
                    {
                        partner.CancelTrade(TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT);
                        chr.message("Partner cannot hold more than one one-of-a-kind item at a time.");
                    }
                    return;
                }

                if (local.getChr().getLevel() < 15)
                {
                    if (local.getChr().getMesosTraded() + partner.Meso > 1000000)
                    {
                        local.CancelTrade(TradeResult.NO_RESPONSE);
                        local.getChr().sendPacket(PacketCreator.serverNotice(1, "Characters under level 15 may not trade more than 1 million mesos per day."));
                        return;
                    }
                    else
                    {
                        local.getChr().addMesosTraded(partner.Meso);
                    }
                }
                else if (partner.getChr().getLevel() < 15)
                {
                    if (partner.getChr().getMesosTraded() + local.Meso > 1000000)
                    {
                        partner.CancelTrade(TradeResult.NO_RESPONSE);
                        partner.getChr().sendPacket(PacketCreator.serverNotice(1, "Characters under level 15 may not trade more than 1 million mesos per day."));
                        return;
                    }
                    else
                    {
                        partner.getChr().addMesosTraded(local.Meso);
                    }
                }

                logTrade(local, partner);
                local.GainItemByComplete();
                partner.GainItemByComplete();

                partner.getChr().setTrade(null);
                chr.setTrade(null);
            }
        }
    }
}
