using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Invites;
using Application.Resources.Messages;
using Application.Shared.Invitations;
using client.inventory;
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

        public static async Task CancelTrade(Player chr, TradeResult result)
        {
            var trade = chr.getTrade();
            if (trade != null)
            {
                await trade.CancelTrade(result);
            }
        }

        public static bool StartTrade(Player chr)
        {
            if (chr.getTrade() == null)
            {
                chr.setTrade(new Trade(0, chr));
                return true;
            }
            return false;
        }

        private static bool hasTradeInviteBack(Player c1, Player c2)
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

        public static async Task InviteTrade(Player c1, Player c2)
        {

            if (c1.isGM() && !c2.isGM() && c1.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_TRADE)
            {
                await c1.MessageI18N(nameof(ClientMessage.Trade_GMCheck));
                log.Information("GM {GMName} blocked from trading with {CharacterName} due to GM level.", c1.getName(), c2.getName());
                await CancelTrade(c1, TradeResult.NO_RESPONSE);
                return;
            }

            if (!c1.isGM() && c2.isGM() && c2.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_TRADE)
            {
                await c1.MessageI18N(nameof(ClientMessage.Trade_GMCheck2));
                await CancelTrade(c1, TradeResult.NO_RESPONSE);
                return;
            }

            if (InviteType.TRADE.HasRequest(c1.getId()))
            {
                if (hasTradeInviteBack(c1, c2))
                {
                    await c1.MessageI18N(nameof(ClientMessage.Trade_AlreadyManaged));
                }
                else
                {
                    await c1.MessageI18N(nameof(ClientMessage.Trade_AlreadyManaged2));
                }

                return;
            }
            else if (c1.getTrade()!.isFullTrade())
            {
                await c1.MessageI18N(nameof(ClientMessage.Trade_Ing));
                return;
            }

            if (InviteType.TRADE.CreateInvite(new LocalInviteRequest(c1, c2)))
            {
                if (JoinTrade(c1.getTrade()!, c2))
                {
                    await c1.SendPacket(PacketCreator.getTradeStart(c1.getClient(), c1.getTrade()!, 0));
                    await c2.SendPacket(PacketCreator.tradeInvite(c1));
                }
                else
                {
                    await c1.MessageI18N(nameof(ClientMessage.Trade_TargetInTrading));
                    await CancelTrade(c1, TradeResult.NO_RESPONSE);
                    InviteType.TRADE.AnswerInvite(c2.getId(), c1.getId(), false);
                }
            }
            else
            {
                await c1.MessageI18N(nameof(ClientMessage.Trade_TargetInTradeManaging));
                await CancelTrade(c1, TradeResult.NO_RESPONSE);
            }
        }

        public static bool JoinTrade(Trade trade, Player player)
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

        public static async Task VisitTrade(Player c1, Player c2)
        {
            var inviteRes = InviteType.TRADE.AnswerInvite(c1.getId(), c2.getId(), true);

            InviteResultType res = inviteRes.Result;
            if (res == InviteResultType.ACCEPTED)
            {
                if (c1.getTrade() != null && c1.getTrade()!.PartnerTrade == c2.getTrade() && c2.getTrade() != null && c2.getTrade()!.PartnerTrade == c1.getTrade())
                {
                    await c2.SendPacket(PacketCreator.getTradePartnerAdd(c1));
                    await c1.SendPacket(PacketCreator.getTradeStart(c1.getClient(), c1.getTrade()!, 1));
                    c1.getTrade()!.setFullTrade(true);
                    c2.getTrade()!.setFullTrade(true);
                }
                else
                {
                    await c1.MessageI18N(nameof(ClientMessage.Trade_AlreadyClosed));
                }
            }
            else
            {
                await c1.MessageI18N(nameof(ClientMessage.Trade_InviteCanceld));
                await CancelTrade(c1, TradeResult.NO_RESPONSE);
            }
        }

        public static async Task DeclineTrade(Player chr)
        {
            var trade = chr.getTrade();
            if (trade != null)
            {
                if (trade.PartnerTrade != null)
                {
                    Player other = trade.PartnerTrade.getChr();
                    if (InviteType.TRADE.AnswerInvite(chr.Id, other.Id, false).Result == InviteResultType.DENIED)
                    {
                        await other.Pink(chr.getName() + " has declined your trade request.");
                    }

                    await other.getTrade()!.GainItemByCancel((byte)TradeResult.PARTNER_CANCEL);
                    other.setTrade(null);

                }
                await trade.GainItemByCancel((byte)TradeResult.NO_RESPONSE);
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
            return string.Join(", ", items.Select(x => $"[{x.getQuantity()} {ClientCulture.SystemCulture.GetItemName(x.getItemId())} ({x.getItemId()})]"));
        }

        public static async Task CompleteTrade(Player chr)
        {
            var local = chr.getTrade();
            if (local == null || local.PartnerTrade == null)
                return;

            if (await local.checkCompleteHandshake())
            {
                var partner = local.PartnerTrade;
                if (!local.fitsMeso())
                {
                    await local.CancelTrade(TradeResult.UNSUCCESSFUL);
                    await chr.MessageI18N(nameof(ClientMessage.Trade_MesoExceedLimit));
                    await partner.getChr().MessageI18N(nameof(ClientMessage.Trade_MesoExceedLimit2));
                    return;
                }
                else if (!partner.fitsMeso())
                {
                    await partner.CancelTrade(TradeResult.UNSUCCESSFUL);
                    await chr.MessageI18N(nameof(ClientMessage.Trade_MesoExceedLimit2));
                    await partner.getChr().MessageI18N(nameof(ClientMessage.Trade_MesoExceedLimit));
                    return;
                }

                if (!local.fitsInInventory())
                {
                    if (local.fitsUniquesInInventory())
                    {
                        await local.CancelTrade(TradeResult.UNSUCCESSFUL);
                        await chr.MessageI18N(nameof(ClientMessage.Trade_InventoryExceedLimit));
                        await partner.getChr().MessageI18N(nameof(ClientMessage.Trade_InventoryExceedLimit2));
                    }
                    else
                    {
                        await local.CancelTrade(TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT);
                        await partner.getChr().MessageI18N(nameof(ClientMessage.Trade_UniqueLimit));
                    }
                    return;
                }
                else if (!partner.fitsInInventory())
                {
                    if (partner.fitsUniquesInInventory())
                    {
                        await partner.CancelTrade(TradeResult.UNSUCCESSFUL);
                        await chr.MessageI18N(nameof(ClientMessage.Trade_InventoryExceedLimit2));
                        await partner.getChr().MessageI18N(nameof(ClientMessage.Trade_InventoryExceedLimit));
                    }
                    else
                    {
                        await partner.CancelTrade(TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT);
                        await chr.MessageI18N(nameof(ClientMessage.Trade_UniqueLimit));
                    }
                    return;
                }

                if (local.getChr().getLevel() < 15)
                {
                    if (local.getChr().getMesosTraded() + partner.Meso > 1000000)
                    {
                        await local.CancelTrade(TradeResult.NO_RESPONSE);
                        await local.getChr().SendPacket(PacketCreator.serverNotice(1, "Characters under level 15 may not trade more than 1 million mesos per day."));
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
                        await partner.CancelTrade(TradeResult.NO_RESPONSE);
                        await partner.getChr().SendPacket(PacketCreator.serverNotice(1, "Characters under level 15 may not trade more than 1 million mesos per day."));
                        return;
                    }
                    else
                    {
                        partner.getChr().addMesosTraded(local.Meso);
                    }
                }

                logTrade(local, partner);
                await local.GainItemByComplete();
                await partner.GainItemByComplete();

                partner.getChr().setTrade(null);
                chr.setTrade(null);
            }
        }
    }
}
