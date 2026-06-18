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


using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using server.life;
using server.maps;
using server.partyquest;
using tools;

namespace scripting.reactor;


/**
 * @author Lerk
 * @author Ronan
 */
public class ReactorActionManager : AbstractPlayerInteraction
{
    private Reactor reactor;

    public ReactorActionManager(IChannelClient c, Reactor reactor) : base(c)
    {
        this.reactor = reactor;
    }

    public override IMap getMap()
    {
        return reactor.getMap();
    }
    public override int getMapId()
    {
        return reactor.getMap().getId();
    }

    public async Task hitReactor()
    {
        await reactor.hitReactor(c);
    }

    public async Task destroyNpc(int npcId)
    {
        await reactor.getMap().destroyNPC(npcId);
    }

    private static List<DropEntry> assembleReactorDropEntries(Player chr, List<DropEntry> items)
    {
        DropEntry.ClassifyDropEntries(items, out var dropEntry, out var visibleQuestEntry, out var otherQuestEntry, chr);

        Collections.shuffle(dropEntry);
        Collections.shuffle(visibleQuestEntry);
        Collections.shuffle(otherQuestEntry);

        items.Clear();
        items.AddRange(dropEntry);
        items.AddRange(visibleQuestEntry);
        items.AddRange(otherQuestEntry);

        List<DropEntry> items1 = new(items.Count);
        List<DropEntry> items2 = new(items.Count / 2);

        for (int i = 0; i < items.Count; i++)
        {
            if (i % 2 == 0)
            {
                items1.Add(items.get(i));
            }
            else
            {
                items2.Add(items.get(i));
            }
        }

        items1.Reverse();
        items1.AddRange(items2);

        return items1;
    }

    public async Task dropItems()
    {
        await dropItems(false, 0, 0, 0, 0);
    }

    public async Task dropItems(bool meso, int mesoChance, int minMeso, int maxMeso, int minItems = 0)
    {
        await dropItems(reactor.getPosition().X, reactor.getPosition().Y, meso, mesoChance, minMeso, maxMeso, minItems);
    }

    public async Task dropItems(int posX, int posY, bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
    {
        await dropItems(true, posX, posY, meso, mesoChance, minMeso, maxMeso, minItems);  // all reactors actually drop items sequentially... thanks inhyuk for pointing this out!
    }

    public async Task dropItems(bool delayed, int posX, int posY, bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
    {
        var chr = c.OnlinedCharacter;
        if (chr == null)
        {
            return;
        }

        var items = assembleReactorDropEntries(chr, generateDropList(getDropChances(), chr.getDropRate(), meso, mesoChance, minItems));
        if (items.Count % 2 == 0)
        {
            posX -= 12;
        }
        Point dropPos = new Point(posX, posY);
        var worldMesoRate = c.CurrentServer.WorldMesoRate;

        if (!delayed)
        {
            byte p = 1;
            foreach (var d in items)
            {
                dropPos.X = posX + ((p % 2 == 0) ? (25 * ((p + 1) / 2)) : -(25 * (p / 2)));
                p++;

                await DropInternal(d, minMeso, maxMeso, worldMesoRate, dropPos, chr, 0);
            }
        }
        else
        {
            dropPos.X -= (12 * items.Count);
            short delay = 0;

            foreach (var d in items)
            {
                await DropInternal(d, minMeso, maxMeso, worldMesoRate, dropPos, chr, delay);
                dropPos.X += 25;
                delay += 200;
            }


        }
    }

    private async Task DropInternal(DropEntry d, int minMeso, int maxMeso, float worldMesoRate, Point dropPos, Player chr, short delay)
    {
        if (d.ItemId == 0)
        {
            var map = reactor.getMap();
            int baseDrop = d.GetRandomCount(minMeso, maxMeso);
            int mesoDrop = (int)(baseDrop * worldMesoRate);
            if (mesoDrop > 0)
                await map.spawnMesoDrop(mesoDrop, dropPos, reactor, chr, false, DropType.FreeForAll, delay);
        }
        else
        {
            var drop = ItemInformationProvider.getInstance().GenerateVirtualItemById(d.ItemId, 1, true);
            if (drop != null)
                await reactor.getMap().dropFromReactor(getPlayer(), reactor, drop, dropPos, d.QuestId, delay);
        }
    }

    private List<DropEntry> getDropChances()
    {
        return c.CurrentServer.ReactorScriptManager.getDrops(reactor.getId());
    }

    private List<DropEntry> generateDropList(List<DropEntry> drops, float dropRate, bool meso, int mesoChance, int minItems)
    {
        List<DropEntry> items = new();
        if (meso && Randomizer.nextDouble() < (1 / (double)mesoChance))
        {
            items.Add(DropEntry.ReactorDropMeso(mesoChance));
        }

        foreach (var mde in drops)
        {
            if (Randomizer.nextDouble() < (dropRate / (double)mde.Chance))
            {
                items.Add(mde);
            }
        }

        while (items.Count < minItems)
        {
            items.Add(DropEntry.ReactorDropMeso(mesoChance));
        }

        return items;
    }

    public async Task spawnMonster(int id)
    {
        await spawnMonster(id, 1, getPosition());
    }

    public async Task createMapMonitor(int mapId, string portal)
    {
        await new MapMonitor(await c.CurrentServer.getMapFactory().getMap(mapId), portal).Initialize();
    }

    public async Task spawnMonster(int id, int qty)
    {
        await spawnMonster(id, qty, getPosition());
    }

    public async Task spawnMonster(int id, int qty, int x, int y)
    {
        await spawnMonster(id, qty, new Point(x, y));
    }

    public async Task spawnMonster(int id, int qty, Point pos)
    {
        for (int i = 0; i < qty; i++)
        {
            var monster = LifeFactory.Instance.getMonster(id);
            if (monster == null)
            {
                Log.Logger.Fatal("Monster (Id {MonsterId}) not found", id);
                continue;
            }
            await reactor.getMap().spawnMonsterOnGroundBelow(monster, pos);
        }
    }

    public async Task killMonster(int id, bool withDrops = false)
    {
        await getMap().killMonster(id, withDrops);
    }

    public Point getPosition()
    {
        Point pos = reactor.getPosition();
        pos.Y -= 10;
        return pos;
    }

    public async Task spawnNpc(int npcId)
    {
        await spawnNpc(npcId, getPosition());
    }

    public async Task spawnNpc(int npcId, Point pos)
    {
        await spawnNpc(npcId, pos, reactor.getMap());
    }

    public Reactor getReactor()
    {
        return reactor;
    }

    public async Task spawnFakeMonster(int id)
    {
        var monster = LifeFactory.Instance.getMonster(id);
        if (monster == null)
        {
            Log.Logger.Fatal("Monster (Id {MonsterId}) not found", id);
            return;
        }
        await reactor.getMap().spawnFakeMonsterOnGroundBelow(monster, getPosition());
    }

    public async Task SpawnZakum()
    {
        await reactor.getMap().SpawnZakumOnGroundBelow(getPosition());
    }

    /**
     * Used for Targa and Scarlion
     */
    public async Task summonBossDelayed(int mobId, int delayMs, int x, int y, string bgm,
                                  string summonMessage)
    {

        await c.CurrentServer.TimerManager.schedule(() =>
         {
             reactor.getMap().Send(async map =>
             {
                 await map.spawnMonsterOnGroundBelow(mobId, x, y);
                 await map.broadcastMessage(PacketCreator.musicChange(bgm));
                 await map.LightBlue(summonMessage);
             });
         }, delayMs);
    }

    public async Task dispelAllMonsters(int num, int team)
    {
        //dispels all mobs, cpq
        var skil = CarnivalFactory.getInstance().getGuardian(num);
        if (skil != null)
        {
            await getMap().ProcessMonster(async mons =>
            {
                if (mons.getTeam() == team)
                {
                    await mons.dispelSkill(skil.getSkill());
                }
            });

            if (getPlayer().getMap() is ICPQMap map)
            {
                if (team == 0)
                {
                    map.getRedTeamBuffs().Remove(skil);
                }
                else
                {
                    map.getBlueTeamBuffs().Remove(skil);
                }
            }

        }
    }
}