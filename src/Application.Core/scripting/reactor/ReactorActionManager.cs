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


using Application.Core.scripting.Event;
using client.inventory;
using constants.inventory;
using Microsoft.ClearScript.V8;
using server;
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
    private V8ScriptEngine iv;
    private ScheduledFuture? sprayTask = null;

    public ReactorActionManager(IClient c, Reactor reactor, V8ScriptEngine iv) : base(c)
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

    private static void sortDropEntries(List<ReactorDropEntry> from, List<ReactorDropEntry> item, List<ReactorDropEntry> visibleQuest, List<ReactorDropEntry> otherQuest, IPlayer chr)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        foreach (ReactorDropEntry mde in from)
        {
            if (!ii.isQuestItem(mde.itemId))
            {
                item.Add(mde);
            }
            else
            {
                if (chr.needQuestItem(mde.questid, mde.itemId))
                {
                    visibleQuest.Add(mde);
                }
                else
                {
                    otherQuest.Add(mde);
                }
            }
        }
    }

    private static List<ReactorDropEntry> assembleReactorDropEntries(IPlayer chr, List<ReactorDropEntry> items)
    {
        List<ReactorDropEntry> dropEntry = new();
        List<ReactorDropEntry> visibleQuestEntry = new();
        List<ReactorDropEntry> otherQuestEntry = new();
        sortDropEntries(items, dropEntry, visibleQuestEntry, otherQuestEntry, chr);

        Collections.shuffle(dropEntry);
        Collections.shuffle(visibleQuestEntry);
        Collections.shuffle(otherQuestEntry);

        items.Clear();
        items.AddRange(dropEntry);
        items.AddRange(visibleQuestEntry);
        items.AddRange(otherQuestEntry);

        List<ReactorDropEntry> items1 = new(items.Count);
        List<ReactorDropEntry> items2 = new(items.Count / 2);

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

    public void sprayItems(bool meso, int mesoChance, int minMeso, int maxMeso)
    {
        sprayItems(meso, mesoChance, minMeso, maxMeso, 0);
    }

    public void sprayItems(bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
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

    public void dropItems(bool meso, int mesoChance, int minMeso, int maxMeso)
    {
        dropItems(meso, mesoChance, minMeso, maxMeso, 0);
    }

    public void dropItems(bool meso, int mesoChance, int minMeso, int maxMeso, int minItems)
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

        List<ReactorDropEntry> items = assembleReactorDropEntries(chr, generateDropList(getDropChances(), chr.getDropRate(), meso, mesoChance, minItems));
        if (items.Count % 2 == 0)
        {
            posX -= 12;
        }
        Point dropPos = new Point(posX, posY);

        if (!delayed)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            byte p = 1;
            foreach (ReactorDropEntry d in items)
            {
                dropPos.X = posX + ((p % 2 == 0) ? (25 * ((p + 1) / 2)) : -(25 * (p / 2)));
                p++;

                if (d.itemId == 0)
                {
                    int range = maxMeso - minMeso;
                    int displayDrop = (int)(Randomizer.nextDouble() * range) + minMeso;
                    int mesoDrop = (displayDrop * c.getWorldServer().MesoRate);
                    reactor.getMap().spawnMesoDrop(mesoDrop, reactor.getMap().calcDropPos(dropPos, reactor.getPosition()), reactor, c.OnlinedCharacter, false, 2);
                }
                else
                {
                    Item drop;

                    if (ItemConstants.getInventoryType(d.itemId) != InventoryType.EQUIP)
                    {
                        drop = new Item(d.itemId, 0, 1);
                    }
                    else
                    {
                        drop = ii.randomizeStats((Equip)ii.getEquipById(d.itemId));
                    }

                    reactor.getMap().dropFromReactor(getPlayer(), reactor, drop, dropPos, (short)d.questid);
                }
            }
        }
        else
        {
            Reactor r = reactor;
            List<ReactorDropEntry> dropItems = items;
            int worldMesoRate = c.getWorldServer().MesoRate;

            dropPos.X -= (12 * items.Count);

            sprayTask = TimerManager.getInstance().register(() =>
            {
                if (dropItems.Count == 0)
                {
                    sprayTask!.cancel(false);
                    return;
                }

                ReactorDropEntry d = dropItems.remove(0);
                if (d.itemId == 0)
                {
                    int range = maxMeso - minMeso;
                    int displayDrop = (int)(Randomizer.nextDouble() * range) + minMeso;
                    int mesoDrop = (displayDrop * worldMesoRate);
                    r.getMap().spawnMesoDrop(mesoDrop, r.getMap().calcDropPos(dropPos, r.getPosition()), r, chr, false, 2);
                }
                else
                {
                    Item drop;

                    if (ItemConstants.getInventoryType(d.itemId) != InventoryType.EQUIP)
                    {
                        drop = new Item(d.itemId, 0, 1);
                    }
                    else
                    {
                        ItemInformationProvider ii = ItemInformationProvider.getInstance();
                        drop = ii.randomizeStats((Equip)ii.getEquipById(d.itemId));
                    }

                    r.getMap().dropFromReactor(getPlayer(), r, drop, dropPos, (short)d.questid);
                }

                dropPos.X += 25;
            }, 200);
        }
    }

    private List<ReactorDropEntry> getDropChances()
    {
        return ReactorScriptManager.getInstance().getDrops(reactor.getId());
    }

    private List<ReactorDropEntry> generateDropList(List<ReactorDropEntry> drops, int dropRate, bool meso, int mesoChance, int minItems)
    {
        List<ReactorDropEntry> items = new();
        if (meso && Randomizer.nextDouble() < (1 / (double)mesoChance))
        {
            items.Add(new ReactorDropEntry(0, mesoChance, -1));
        }

        foreach (ReactorDropEntry mde in drops)
        {
            if (Randomizer.nextDouble() < (dropRate / (double)mde.chance))
            {
                items.Add(mde);
            }
        }

        while (items.Count < minItems)
        {
            items.Add(new ReactorDropEntry(0, mesoChance, -1));
        }

        return items;
    }

    public void spawnMonster(int id)
    {
        spawnMonster(id, 1, getPosition());
    }

    public void createMapMonitor(int mapId, string portal)
    {
        new MapMonitor(c.getChannelServer().getMapFactory().getMap(mapId), portal);
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
            var monster = LifeFactory.getMonster(id);
            if (monster == null)
            {
                Log.Logger.Fatal("Monster (Id {MonsterId}) not found", id);
                continue;
            }
            reactor.getMap().spawnMonsterOnGroundBelow(monster, pos);
        }
    }

    public void killMonster(int id)
    {
        killMonster(id, false);
    }

    public void killMonster(int id, bool withDrops)
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
        var monster = LifeFactory.getMonster(id);
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
        TimerManager.getInstance().schedule(() =>
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
    { //dispels all mobs, cpq
        var skil = CarnivalFactory.getInstance().getGuardian(num);
        if (skil != null)
        {
            foreach (var mons in getMap().getAllMonsters())
            {
                if (mons.getTeam() == team)
                {
                    mons.dispelSkill(skil.getSkill());
                }
            }

            if (team == 0)
            {
                getPlayer().getMap().getRedTeamBuffs().Remove(skil);
            }
            else
            {
                getPlayer().getMap().getBlueTeamBuffs().Remove(skil);
            }
        }
    }
}