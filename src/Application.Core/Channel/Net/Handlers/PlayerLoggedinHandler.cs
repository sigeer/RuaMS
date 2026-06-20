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


using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Game.Skills;
using Application.Shared.KeyMaps;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;


public class PlayerLoggedinHandler : ChannelHandlerBase
{
    readonly ILogger<ChannelHandlerBase> _logger;
    readonly DataService _dataService;
    readonly TeamManager _teamManger;
    public PlayerLoggedinHandler(ILogger<ChannelHandlerBase> logger, DataService dataService, TeamManager teamManager)
    {
        _logger = logger;
        _dataService = dataService;
        _teamManger = teamManager;
    }
    public override bool ValidateState(IChannelClient c)
    {
        return !c.IsOnlined;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        int cid = p.readInt(); // TODO: investigate if this is the "client id" supplied in PacketCreator#getServerIP()

        await c.tryacquireClient();
        try
        {
            var cserv = c.CurrentServer;
            if (cserv == null || !cserv.IsRunning)
            {
                await c.Disconnect(true, false);
                return;
            }

            var playerObject = _dataService.GetPlayerData(cserv.getId(), c.GetSessionRemoteHost(), cid);
            if (playerObject == null)
            {
                await c.Disconnect(true, false);
                return;
            }

            c.Hwid = new Hwid(playerObject.Account.CurrentHwid);

            var player = await _dataService.Serialize(c, playerObject);
            if (player == null)
            {
                // 1. 玩家不存在 2. 玩家并不处于切换服务器状态
                await c.Disconnect(true, false);
                return;
            }
            c.Language = playerObject.LoginInfo.Language;
            c.CurrentCulture = new ClientCulture(c.Language);
            bool newcomer = playerObject.LoginInfo.IsNewCommer;

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

            await cserv.addPlayer(player);

            await _dataService.RecoverCharacterBuff(player);

            await c.SendPacket(PacketCreator.getCharInfo(player));
            if (!player.isHidden())
            {
                if (player.isGM() && YamlConfig.config.server.USE_AUTOHIDE_GM)
                {
                    await player.toggleHide(true);
                }
            }
            await player.sendKeymap();
            await player.sendQuickmap();
            await player.sendMacros();

            // pot bindings being passed through other characters on the account detected thanks to Croosade dev team
            var autohpPot = player.KeyMap.GetData((int)KeyCode.VirtualAutoPotionHP);
            await player.SendPacket(PacketCreator.sendAutoHpPot(autohpPot != null ? autohpPot.getAction() : 0));

            var autompPot = player.KeyMap.GetData((int)KeyCode.VirtualAutoPotionMP);
            await player.SendPacket(PacketCreator.sendAutoMpPot(autompPot != null ? autompPot.getAction() : 0));

            await player.getMap().addPlayer(player);

            await c.SendPacket(PacketCreator.updateBuddylist(player.BuddyList.getBuddies()));

            // 有必要？
            await c.SendPacket(PacketCreator.updateGender(player));

            //退出游戏/切换频道会退出聊天室，那这里的方法又有什么用？
            // player.checkMessenger();
            await c.SendPacket(PacketCreator.enableReport());
            await player.changeSkillLevel(SkillFactory.GetSkillTrust(10000000 * player.getJobType() + 12), (sbyte)(player.getLinkedLevel() / 10), 20, -1);

            if (newcomer)
            {
                var mount = player.getMount();   // thanks Ari for noticing a scenario where Silver Mane quest couldn't be started
                if (mount != null && mount.getItemId() != 0)
                {
                    await player.SendPacket(PacketCreator.updateMount(player.getId(), mount, false));
                }

                await player.reloadQuestExpirations();
            }
            else
            {
                if (player.isRidingBattleship())
                {
                    await player.announceBattleshipHp();
                }
            }

            if (player.JobModel.HasDragon())
            {
                player.createDragon();
            }

            await player.commitExcludedItems();


            if (newcomer)
            {
                await c.CurrentServer.EventRecallManager.recallEventInstance(player);
            }

            // Tell the client to use the custom scripts available for the NPCs provided, instead of the WZ entries.
            if (YamlConfig.config.server.USE_NPCS_SCRIPTABLE)
            {

                // Create a copy to prevent always adding entries to the server's list.
                Dictionary<int, string> npcsIds = YamlConfig.config.server.NPCS_SCRIPTABLE.Select(x => new KeyValuePair<int, string>(int.Parse(x.Key), x.Value)).ToDictionary();

                await c.SendPacket(PacketCreator.setNPCScriptable(npcsIds));
            }

            await _dataService.CompleteLogin(c.CurrentServer, player, playerObject);
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
}
