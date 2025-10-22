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
using Application.Core.Game.Maps.Specials;
using client.inventory;
using server.life;
using server.maps;
using server.partyquest;

namespace scripting.reactor;


/**
 * @author Lerk
 * @author Ronan
 */
public class ReactorActionManager : AbstractPlayerInteraction
{
    private Reactor reactor;
    private IEngine iv;

    public ReactorActionManager(IChannelClient c, Reactor reactor, IEngine iv) : base(c)
    {
        this.reactor = reactor;
        this.iv = iv;
    }

    public void hitReactor()
    {
        reactor.hitReactor(c);
    }

    public void destroyNpc(int npcId)
    {
        reactor.getMap().destroyNPC(npcId);
    }

    private static List<DropEntry> assembleReactorDropEntries(IPlayer chr, List<DropEntry> items)
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

    public void sprayItems()
    {
        sprayItems(false, 0, 0, 0, 0);
    }

    public void sprayItems(bool meso, int mesoChance, int minMeso, int maxMeso, int minItems = 0)
    {
        sprayItems(reactor.getPosition().X, reactor.getPosition().Y, meso, mesoChance, minMeso, maxMeso, minItems);
    }

    public void sprayItems(int posX, int posY, bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
    {
        dropItems(true, posX, posY, meso, mesoChance, minMeso, maxMeso, minItems);
    }

    public void dropItems()
    {
        dropItems(false, 0, 0, 0, 0);
    }

    public void dropItems(bool meso, int mesoChance, int minMeso, int maxMeso, int minItems = 0)
    {
        dropItems(reactor.getPosition().X, reactor.getPosition().Y, meso, mesoChance, minMeso, maxMeso, minItems);
    }

    public void dropItems(int posX, int posY, bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
    {
        dropItems(true, posX, posY, meso, mesoChance, minMeso, maxMeso, minItems);  // all reactors actually drop items sequentially... thanks inhyuk for pointing this out!
    }

    public void dropItems(bool delayed, int posX, int posY, bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
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

                DropInternal(d, minMeso, maxMeso, worldMesoRate, dropPos, chr, 0);
            }
        }
        else
        {
            dropPos.X -= (12 * items.Count);
            short delay = 0;

            foreach (var d in items)
            {
                DropInternal(d, minMeso, maxMeso, worldMesoRate, dropPos, chr, delay);
                dropPos.X += 25;
                delay += 200;
            }


        }
    }

    private void DropInternal(DropEntry d, int minMeso, int maxMeso, float worldMesoRate, Point dropPos, IPlayer chr, short delay)
    {
        if (d.ItemId == 0)
        {
            var map = reactor.getMap();
            int baseDrop = d.GetRandomCount(minMeso, maxMeso);
            int mesoDrop = (int)(baseDrop * worldMesoRate);
            if (mesoDrop > 0)
                map.spawnMesoDrop(mesoDrop, dropPos, reactor, chr, false, DropType.FreeForAll, delay);
        }
        else
        {
            Item drop;

            if (ItemConstants.getInventoryType(d.ItemId) != InventoryType.EQUIP)
            {
                drop = new Item(d.ItemId, 0, 1);
            }
            else
            {
                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                drop = ii.randomizeStats((Equip)ii.getEquipById(d.ItemId));
            }

            reactor.getMap().dropFromReactor(getPlayer(), reactor, drop, dropPos, d.QuestId, delay);
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

    public void spawnMonster(int id)
    {
        spawnMonster(id, 1, getPosition());
    }

    public void createMapMonitor(int mapId, string portal)
    {
        new MapMonitor(c.CurrentServer.getMapFactory().getMap(mapId), portal);
    }

    public void spawnMonster(int id, int qty)
    {
        spawnMonster(id, qty, getPosition());
    }

    public void spawnMonster(int id, int qty, int x, int y)
    {
        spawnMonster(id, qty, new Point(x, y));
    }

    public void spawnMonster(int id, int qty, Point pos)
    {
        for (int i = 0; i < qty; i++)
        {
            var monster = LifeFactory.Instance.getMonster(id);
            if (monster == null)
            {
                Log.Logger.Fatal("Monster (Id {MonsterId}) not found", id);
                continue;
            }
            reactor.getMap().spawnMonsterOnGroundBelow(monster, pos);
        }
    }

    public void killMonster(int id, bool withDrops = false)
    {
        if (withDrops)
        {
            getMap().killMonsterWithDrops(id);
        }
        else
        {
            getMap().killMonster(id);
        }
    }

    public Point getPosition()
    {
        Point pos = reactor.getPosition();
        pos.Y -= 10;
        return pos;
    }

    public void spawnNpc(int npcId)
    {
        spawnNpc(npcId, getPosition());
    }

    public void spawnNpc(int npcId, Point pos)
    {
        spawnNpc(npcId, pos, reactor.getMap());
    }

    public Reactor getReactor()
    {
        return reactor;
    }

    public void spawnFakeMonster(int id)
    {
        var monster = LifeFactory.Instance.getMonster(id);
        if (monster == null)
        {
            Log.Logger.Fatal("Monster (Id {MonsterId}) not found", id);
            return;
        }
        reactor.getMap().spawnFakeMonsterOnGroundBelow(monster, getPosition());
    }

    /**
     * Used for Targa and Scarlion
     */
    public void summonBossDelayed(int mobId, int delayMs, int x, int y, string bgm,
                                  string summonMessage)
    {
        c.CurrentServerContainer.TimerManager.schedule(() =>
        {
            summonBoss(mobId, x, y, bgm, summonMessage);
        }, delayMs);
    }

    private void summonBoss(int mobId, int x, int y, string bgmName, string summonMessage)
    {
        spawnMonster(mobId, x, y);
        changeMusic(bgmName);
        mapMessage(6, summonMessage);
    }

    public void dispelAllMonsters(int num, int team)
    {
        //dispels all mobs, cpq
        var skil = CarnivalFactory.getInstance().getGuardian(num);
        if (skil != null)
        {
            getMap().ProcessMonster(mons =>
            {
                if (mons.getTeam() == team)
                {
                    mons.dispelSkill(skil.getSkill());
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