/*
 This file is part of the OdinMS Maple Story Server
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


using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Core.Server.maps;
using tools;

namespace scripting.portal;

public class PortalPlayerInteraction : AbstractPlayerInteraction
{
    private IPortal portal;

    public PortalPlayerInteraction(IChannelClient c, IPortal portal) : base(c)
    {
        this.portal = portal;
    }

    public IPortal getPortal()
    {
        return portal;
    }

    public bool hasLevel30Character()
    {
        return c.OnlinedCharacter.getLinkedLevel() >= 30;
    }

    public async Task blockPortal()
    {
        await c.OnlinedCharacter.blockPortal(getPortal().getScriptName());
    }

    public void unblockPortal()
    {
        c.OnlinedCharacter.unblockPortal(getPortal().getScriptName());
    }

    public async Task playPortalSound()
    {
        await c.SendPacket(PacketCreator.playPortalSound());
    }

    public bool CanEnterDeveloperRoom()
    {
        return c.CurrentServer.NodeService.AdminService.GetServerStats().IsDevRoomAvailable;
    }

    /// <summary>
    /// 开始单人任务副本并回馈结果。
    /// private：脚本注册表收集的是 public 方法，辅助方法必须私有，否则会被当成脚本键。
    /// </summary>
    /// <param name="questId">任务 Id，同时也是 <see cref="SoloQuestEventTemplate"/> 的注册键（q{questId}）</param>
    /// <returns>true 表示已进入副本</returns>
    protected Task<bool> TryStartSoloQuest(int questId)
    {
        return TryStartEvent(GetSoloQuestEventManager(questId));
    }

    /// <summary>
    /// 开始任意事件副本并回馈结果。
    /// private 理由同 <see cref="TryStartSoloQuest"/>。
    /// </summary>
    protected async Task<bool> TryStartEvent(AbstractEventManager em)
    {
        var r = await em.StartInstance(getPlayer());
        if (r == CreateInstanceResult.Success)
        {
            await playPortalSound();
            return true;
        }

        await Pink(em.HandleCreateInstanceResult(r, c));
        return false;
    }

}