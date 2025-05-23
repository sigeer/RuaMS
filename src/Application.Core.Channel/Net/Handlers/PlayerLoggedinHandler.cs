/*
 This file is part of the OdinMS Maple Story NewServer
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Channel.Services;
using Application.Core.Game.Players;
using Application.Core.Game.Skills;
using Application.Core.Managers;
using Application.EF;
using Application.Shared.KeyMaps;
using Application.Utility.Configs;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server;
using net.server.coordinator.session;
using net.server.coordinator.world;
using net.server.guild;
using net.server.world;
using service;
using tools;

namespace Application.Core.Channel.Net.Handlers;


public class PlayerLoggedinHandler : ChannelHandlerBase
{
    private NoteService noteService;
    readonly ILogger<ChannelHandlerBase> _logger;
    readonly CharacterService _characterSrv;
    public PlayerLoggedinHandler(NoteService noteService, ILogger<ChannelHandlerBase> logger, CharacterService characterSrv)
    {
        this.noteService = noteService;
        _logger = logger;
        _characterSrv = characterSrv;
    }
    public override bool ValidateState(IChannelClient c)
    {
        return !c.IsOnlined;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int cid = p.readInt(); // TODO: investigate if this is the "client id" supplied in PacketCreator#getServerIP()
        Server server = Server.getInstance();

        if (!c.tryacquireClient())
        {
            // thanks MedicOP for assisting on concurrency protection here
            c.sendPacket(PacketCreator.getAfterLoginError(10));
        }

        try
        {
            var cserv = c.CurrentServer;
            if (cserv == null || !cserv.IsRunning)
            {
                c.Disconnect(true, false);
                return;
            }

            var playerObject = c.CurrentServer.Service.GetPlayerData(c.GetSessionRemoteHost(), cid);
            if (playerObject == null)
            {
                c.Disconnect(true, false);
                return;
            }

            c.Hwid = new Hwid(playerObject.Account.Hwid);

            var player = _characterSrv.Serialize(c, playerObject);
            if (player == null)
            {
                // 1. 玩家不存在 2. 玩家并不处于切换服务器状态
                c.Disconnect(true, false);
                return;
            }
            bool newcomer = playerObject.LoginInfo!.IsNewCommer;

            /*  is this check really necessary?
            if (state == IChannelClient.LOGIN_SERVER_TRANSITION || state == IChannelClient.LOGIN_NOTLOGGEDIN) {
                List<string> charNames = c.loadCharacterNames(c.getWorld());
                if(!newcomer) {
                    charNames.Remove(player.getName());
                }

                foreach(string charName in charNames) {
                    if(wserv.getPlayerStorage().getCharacterByName(charName) != null) {
                        allowLogin = false;
                        break;
                    }
                }
            }
            */


            var wserv = Server.getInstance().getWorld(0);

            // 换线，离开商城拍卖回到主世界
            if (!newcomer)
            {
                player.LinkNewChannelClient(c);
            }

            cserv.addPlayer(player);

            player.setEnteredChannelWorld(c.Channel);

            c.CurrentServer.RecoverCharacterBuff(player);

            c.sendPacket(PacketCreator.getCharInfo(player));
            if (!player.isHidden())
            {
                if (player.isGM() && YamlConfig.config.server.USE_AUTOHIDE_GM)
                {
                    player.toggleHide(true);
                }
            }
            player.sendKeymap();
            player.sendQuickmap();
            player.sendMacros();

            // pot bindings being passed through other characters on the account detected thanks to Croosade dev team
            var autohpPot = player.KeyMap.GetData((int)KeyCode.VirtualAutoPotionHP);
            player.sendPacket(PacketCreator.sendAutoHpPot(autohpPot != null ? autohpPot.getAction() : 0));

            var autompPot = player.KeyMap.GetData((int)KeyCode.VirtualAutoPotionMP);
            player.sendPacket(PacketCreator.sendAutoMpPot(autompPot != null ? autompPot.getAction() : 0));

            player.getMap().addPlayer(player);
            player.visitMap(player.getMap());

            int[] buddyIds = player.BuddyList.getBuddyIds();
            c.CurrentServer.UpdateBuddyByLoggedIn(player.getId(), c.Channel, buddyIds);
            c.sendPacket(PacketCreator.updateBuddylist(player.BuddyList.getBuddies()));

            c.sendPacket(PacketCreator.loadFamily(player));
            if (player.getFamilyId() > 0)
            {
                var f = wserv.getFamily(player.getFamilyId());
                if (f != null)
                {
                    var familyEntry = f.getEntryByID(player.getId());
                    if (familyEntry != null)
                    {
                        familyEntry.setCharacter(player);
                        player.setFamilyEntry(familyEntry);

                        c.sendPacket(PacketCreator.getFamilyInfo(familyEntry));
                        familyEntry.announceToSenior(PacketCreator.sendFamilyLoginNotice(player.getName(), true), true);
                    }
                    else
                    {
                        _logger.LogError("Chr {CharacterName}'s family doesn't have an entry for them. (familyId {FamilyId})", player.getName(), f.getID());
                    }
                }
                else
                {
                    _logger.LogError("Chr {CharacterName} has an invalid family ID ({FamilyId})", player.getName(), player.getFamilyId());
                    c.sendPacket(PacketCreator.getFamilyInfo(null));
                }
            }
            else
            {
                c.sendPacket(PacketCreator.getFamilyInfo(null));
            }

            if (player.GuildId > 0)
            {
                if (player.GuildModel == null)
                {
                    CharacterManager.deleteGuild(player);
                }
                else
                {
                    player.GuildModel.setOnline(player.Id, true, c.Channel);
                    c.sendPacket(GuildPackets.showGuildInfo(player));
                    if (player.AllianceModel != null)
                    {
                        c.sendPacket(GuildPackets.updateAllianceInfo(player.AllianceModel));
                        c.sendPacket(GuildPackets.allianceNotice(player.AllianceModel.AllianceId, player.AllianceModel.getNotice()));

                        if (newcomer)
                        {
                            player.AllianceModel.broadcastMessage(GuildPackets.allianceMemberOnline(player, true), player.getId(), -1);
                        }
                    }
                    else
                    {
                        player.GuildModel.AllianceId = 0;
                    }
                }
            }

            noteService.show(player);

            if (player.getParty() != null)
            {
                //Use this in case of enabling party HPbar HUD when logging in, however "you created a party" will appear on chat.
                //c.sendPacket(PacketCreator.partyCreated(pchar));
                wserv.updateParty(player.getParty()!.getId(), PartyOperation.LOG_ONOFF, player);
                player.updatePartyMemberHP();
            }

            Inventory eqpInv = player.getInventory(InventoryType.EQUIPPED);
            eqpInv.lockInventory();
            try
            {
                foreach (Item it in eqpInv.list())
                {
                    player.equippedItem((Equip)it);
                }
            }
            finally
            {
                eqpInv.unlockInventory();
            }

            c.sendPacket(PacketCreator.updateBuddylist(player.BuddyList.getBuddies()));

            var pendingBuddyRequest = c.OnlinedCharacter.BuddyList.pollPendingRequest();
            if (pendingBuddyRequest != null)
            {
                c.sendPacket(PacketCreator.requestBuddylistAdd(pendingBuddyRequest.id, c.OnlinedCharacter.getId(), pendingBuddyRequest.name));
            }

            c.sendPacket(PacketCreator.updateGender(player));
            player.checkMessenger();
            c.sendPacket(PacketCreator.enableReport());
            player.changeSkillLevel(SkillFactory.GetSkillTrust(10000000 * player.getJobType() + 12), (sbyte)(player.getLinkedLevel() / 10), 20, -1);
            player.checkBerserk(player.isHidden());

            if (newcomer)
            {
                foreach (var pet in player.getPets())
                {
                    if (pet != null)
                    {
                        c.CurrentServer.PetHungerController.registerPetHunger(player, player.getPetIndex(pet));
                    }
                }

                var mount = player.getMount();   // thanks Ari for noticing a scenario where Silver Mane quest couldn't be started
                if (mount != null && mount.getItemId() != 0)
                {
                    player.sendPacket(PacketCreator.updateMount(player.getId(), mount, false));
                }

                player.reloadQuestExpirations();

                /*
                if (!c.hasVotedAlready()){
                    player.sendPacket(PacketCreator.earnTitleMessage("You can vote now! Vote and earn a vote point!"));
                }
                */
                if (player.isGM())
                {
                    c.CurrentServer.BroadcastWorldGMPacket(PacketCreator.earnTitleMessage((player.gmLevel() < 6 ? "GM " : "Admin ") + player.getName() + " has logged in"));
                }
            }
            else
            {
                if (player.isRidingBattleship())
                {
                    player.announceBattleshipHp();
                }
            }

            player.StartPlayerTask();

            if (player.JobModel.HasDragon())
            {
                player.createDragon();
            }

            player.commitExcludedItems();
            showDueyNotification(c, player);

            player.updateCouponRates();

            player.receivePartyMemberHP();

            if (player.PartnerId > 0)
            {
                c.CurrentServer.NotifyPartner(player.Id);
                //var partner = wserv.getPlayerStorage().getCharacterById(player.PartnerId);

                //if (partner != null && partner.isLoggedinWorld())
                //{
                //    player.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(partner.Id, partner.getMapId()));
                //    partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(player.getId(), player.getMapId()));
                //}
            }

            if (newcomer)
            {
                var eim = EventRecallCoordinator.getInstance().recallEventInstance(cid);
                eim?.registerPlayer(player);
            }

            // Tell the client to use the custom scripts available for the NPCs provided, instead of the WZ entries.
            if (YamlConfig.config.server.USE_NPCS_SCRIPTABLE)
            {

                // Create a copy to prevent always adding entries to the server's list.
                Dictionary<int, string> npcsIds = YamlConfig.config.server.NPCS_SCRIPTABLE.Select(x => new KeyValuePair<int, string>(int.Parse(x.Key), x.Value)).ToDictionary();

                c.sendPacket(PacketCreator.setNPCScriptable(npcsIds));
            }

            if (newcomer)
            {
                player.setLoginTime(DateTimeOffset.UtcNow);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        finally
        {
            c.releaseClient();
        }
    }

    private void showDueyNotification(IChannelClient c, IPlayer player)
    {
        try
        {

            using var dbContext = new DBContext();
            var dbModel = dbContext.Dueypackages.Where(x => x.ReceiverId == player.getId() && x.Checked).OrderByDescending(x => x.Type).FirstOrDefault();

            if (dbModel != null)
            {
                dbContext.Dueypackages.Where(x => x.ReceiverId == player.getId()).ExecuteUpdate(x => x.SetProperty(y => y.Checked, false));

                c.sendPacket(PacketCreator.sendDueyParcelNotification(dbModel.Type));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
}
