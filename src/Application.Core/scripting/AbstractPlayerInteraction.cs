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


using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Scripting.Events;
using Application.Shared.Events;
using Application.Shared.KeyMaps;
using client;
using client.inventory;
using client.inventory.manipulator;
using client.keybind;
using constants.game;
using scripting.Event;
using server.expeditions;
using server.life;
using server.partyquest;
using server.quest;
using tools;
using static Application.Core.Game.Players.Player;


namespace scripting;

public class AbstractPlayerInteraction : IClientMessenger
{

    public IChannelClient c;
    public AbstractPlayerInteraction(IChannelClient c)
    {
        this.c = c;
    }

    public IChannelClient getClient()
    {
        return c;
    }

    public IPlayer getPlayer()
    {
        return c.OnlinedCharacter;
    }

    public IPlayer getChar()
    {
        return c.OnlinedCharacter;
    }

    public int getJobId()
    {
        return getPlayer().getJob().getId();
    }

    public Job getJob()
    {
        return getPlayer().getJob();
    }

    public string GetJobName(Job job)
    {
        return getClient().CurrentCulture.GetJobName(job);
    }

    public int getLevel()
    {
        return getPlayer().getLevel();
    }

    public IMap getMap()
    {
        return c.OnlinedCharacter.getMap();
    }

    public int getHourOfDay()
    {
        return c.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().ToLocalTime().Hour;
    }

    public int getMarketPortalId(int mapId)
    {
        return getMarketPortalId(getWarpMap(mapId));
    }

    private int getMarketPortalId(IMap map)
    {
        return (map.findMarketPortal() != null) ? map.findMarketPortal()!.getId() : map.getRandomPlayerSpawnpoint().getId();
    }

    public void WarpOut()
    {
        warp(getPlayer().getMap().getForcedReturnId());
    }

    public void WarpReturn()
    {
        warp(getPlayer().getMap().getReturnMapId());
    }

    public void WarpReturn(int portal)
    {
        warp(getPlayer().getMap().getReturnMapId(), portal);
    }

    public void warp(int mapid)
    {
        getPlayer().changeMap(mapid);
    }

    public void warp(int map, int portal)
    {
        getPlayer().changeMap(map, portal);
    }

    public void warp(int map, string portal)
    {
        getPlayer().changeMap(map, portal);
    }

    public void warpMap(int map)
    {
        getPlayer().getMap().warpEveryone(map);
    }

    public void warpParty(int id)
    {
        warpParty(id, 0);
    }

    public void warpParty(int id, int portalId)
    {
        int mapid = getMapId();
        warpParty(id, portalId, mapid, mapid);
    }

    public void warpParty(int map, string portalName)
    {

        int mapid = getMapId();
        var warpMap = c.CurrentServer.getMapFactory().getMap(map);

        var portal = warpMap.getPortal(portalName);

        if (portal == null)
        {
            portal = warpMap.getPortal(0)!;
        }

        var portalId = portal.getId();

        warpParty(map, portalId, mapid, mapid);

    }

    public void warpParty(int id, int fromMinId, int fromMaxId)
    {
        warpParty(id, 0, fromMinId, fromMaxId);
    }

    public void warpParty(int id, int portalId, int fromMinId, int fromMaxId)
    {
        foreach (var mc in this.getPlayer().getPartyMembersOnline())
        {
            if (mc.isLoggedinWorld())
            {
                if (mc.getMapId() >= fromMinId && mc.getMapId() <= fromMaxId)
                {
                    mc.changeMap(id, portalId);
                }
            }
        }
    }

    public IMap getWarpMap(int map)
    {
        return getPlayer().getWarpMap(map);
    }

    public IMap getMap(int map)
    {
        return getWarpMap(map);
    }

    public int countAllMonstersOnMap(int map)
    {
        return getMap(map).countMonsters();
    }

    public int countMonster()
    {
        return getPlayer().getMap().countMonsters();
    }

    public void resetMapObjects(int mapid)
    {
        getWarpMap(mapid).resetMapObjects();
    }

    public EventManager? getEventManager(string @event)
    {
        return getClient().getEventManager(@event);
    }

    public AbstractEventInstanceManager? getEventInstance()
    {
        return getPlayer().getEventInstance();
    }

    public Inventory getInventory(int type)
    {
        return getPlayer().getInventory(InventoryTypeUtils.getByType((sbyte)type));
    }

    public Inventory getInventory(InventoryType type)
    {
        return getPlayer().getInventory(type);
    }

    public bool hasItem(int itemid, int quantity = 1)
    {
        return haveItem(itemid, quantity);
    }


    public bool haveItem(int itemid, int quantity = 1)
    {
        return getPlayer().getItemQuantity(itemid, false) >= quantity;
    }

    public int getItemQuantity(int itemid)
    {
        return getPlayer().getItemQuantity(itemid, false);
    }


    public bool haveItemWithId(int itemid, bool checkEquipped = false)
    {
        return getPlayer().haveItemWithId(itemid, checkEquipped);
    }


    public bool canHold(int itemid, int quantity = 1)
    {
        return canHoldAll(Collections.singletonList(itemid), Collections.singletonList(quantity), true);
    }

    public bool canHold(int itemid, int quantity, int removeItemid, int removeQuantity)
    {
        return canHoldAllAfterRemoving(Collections.singletonList(itemid), Collections.singletonList(quantity), Collections.singletonList(removeItemid), Collections.singletonList(removeQuantity));
    }

    private List<int> convertToIntegerList(List<object> objects)
    {
        return objects.Select(x => Convert.ToInt32(x)).ToList();
    }

    public bool canHoldAll(List<object> itemids)
    {
        List<object> quantity = new();

        int intOne = 1;
        for (int i = 0; i < itemids.Count; i++)
        {
            quantity.Add(intOne);
        }

        return canHoldAll(itemids, quantity);
    }

    public bool canHoldAll(List<object> itemids, List<object> quantity)
    {
        return canHoldAll(convertToIntegerList(itemids), convertToIntegerList(quantity), true);
    }

    private bool canHoldAll(List<int> itemids, List<int> quantity, bool isInteger)
    {
        int size = Math.Min(itemids.Count, quantity.Count);

        List<ItemInventoryType> addedItems = new();
        for (int i = 0; i < size; i++)
        {
            Item it = new Item(itemids.get(i), 0, (short)quantity.ElementAtOrDefault(i));
            addedItems.Add(new(it, ItemConstants.getInventoryType(itemids.get(i))));
        }

        return Inventory.checkSpots(c.OnlinedCharacter, addedItems);
    }

    private List<ItemInventoryType> prepareProofInventoryItems(List<ItemQuantity> items)
    {
        List<ItemInventoryType> addedItems = new();
        foreach (var p in items)
        {
            Item it = new Item(p.ItemId, 0, (short)p.Quantity);
            addedItems.Add(new(it, InventoryType.CANHOLD));
        }

        return addedItems;
    }

    private List<List<ItemQuantity>> prepareInventoryItemList(List<int> itemids, List<int> quantity)
    {
        int size = Math.Min(itemids.Count, quantity.Count);

        List<List<ItemQuantity>> invList = new(6);
        for (int i = InventoryType.UNDEFINED.getType(); i <= InventoryType.CASH.getType(); i++)
        {
            invList.Add(new());
        }

        for (int i = 0; i < size; i++)
        {
            int itemid = itemids.get(i);
            invList.get(ItemConstants.getInventoryType(itemid).getType()).Add(new(itemid, quantity.get(i)));
        }

        return invList;
    }

    public bool canHoldAllAfterRemoving(List<int> toAddItemids, List<int> toAddQuantity, List<int> toRemoveItemids, List<int> toRemoveQuantity)
    {
        List<List<ItemQuantity>> toAddItemList = prepareInventoryItemList(toAddItemids, toAddQuantity);
        List<List<ItemQuantity>> toRemoveItemList = prepareInventoryItemList(toRemoveItemids, toRemoveQuantity);

        InventoryProof prfInv = (InventoryProof)this.getInventory(InventoryType.CANHOLD);
        prfInv.lockInventory();
        try
        {
            for (int i = InventoryType.EQUIP.getType(); i < InventoryType.CASH.getType(); i++)
            {
                List<ItemQuantity> toAdd = toAddItemList.get(i);

                if (toAdd.Count > 0)
                {
                    List<ItemQuantity> toRemove = toRemoveItemList.get(i);

                    Inventory inv = this.getInventory(i);
                    prfInv.cloneContents(inv);

                    foreach (var p in toRemove)
                    {
                        InventoryManipulator.removeById(c, InventoryType.CANHOLD, p.ItemId, p.Quantity, false, false);
                    }

                    List<ItemInventoryType> addItems = prepareProofInventoryItems(toAdd);

                    bool canHold = Inventory.checkSpots(c.OnlinedCharacter, addItems, true);
                    if (!canHold)
                    {
                        return false;
                    }
                }
            }
        }
        finally
        {
            prfInv.flushContents();
            prfInv.unlockInventory();
        }

        return true;
    }

    //---- \/ \/ \/ \/ \/ \/ \/  NOT TESTED  \/ \/ \/ \/ \/ \/ \/ \/ \/ ----

    public QuestStatus? getQuestRecord(int id)
    {
        return c.OnlinedCharacter.getQuestNAdd(Quest.getInstance(id));
    }

    public QuestStatus? getQuestNoRecord(int id)
    {
        return c.OnlinedCharacter.getQuestNoAdd(Quest.getInstance(id));
    }

    //---- /\ /\ /\ /\ /\ /\ /\  NOT TESTED  /\ /\ /\ /\ /\ /\ /\ /\ /\ ----

    public void openNpc(int npcid, string? script = null)
    {
        c.OpenNpc(npcid, script);
    }

    public int getQuestStatus(int id)
    {
        return (int)c.OnlinedCharacter.getQuest(Quest.getInstance(id)).getStatus();
    }

    private QuestStatus.Status getQuestStat(int id)
    {
        return c.OnlinedCharacter.getQuest(Quest.getInstance(id)).getStatus();
    }

    public bool isQuestCompleted(int id)
    {
        try
        {
            return getQuestStat(id) == QuestStatus.Status.COMPLETED;
        }
        catch (NullReferenceException e)
        {
            Log.Logger.Error(e.ToString());
            return false;
        }
    }

    public bool isQuestActive(int id)
    {
        return isQuestStarted(id);
    }

    public bool isQuestStarted(int id)
    {
        try
        {
            return getQuestStat(id) == QuestStatus.Status.STARTED;
        }
        catch (NullReferenceException e)
        {
            Log.Logger.Error(e.ToString());
            return false;
        }
    }

    public void setQuestProgress(int id, string progress)
    {
        setQuestProgress(id, 0, progress);
    }

    public void setQuestProgress(int id, int progress)
    {
        setQuestProgress(id, 0, progress.ToString());
    }

    public void setQuestProgress(int id, int infoNumber, int progress)
    {
        setQuestProgress(id, infoNumber, progress.ToString());
    }

    public void setQuestProgress(int id, int infoNumber, string progress)
    {
        c.OnlinedCharacter.setQuestProgress(id, infoNumber, progress);
    }

    public string getQuestProgress(int id)
    {
        return getQuestProgress(id, 0);
    }

    public string getQuestProgress(int id, int infoNumber)
    {
        QuestStatus qs = getPlayer().getQuest(Quest.getInstance(id));

        if (qs.getInfoNumber() == infoNumber && infoNumber > 0)
        {
            qs = getPlayer().getQuest(Quest.getInstance(infoNumber));
            infoNumber = 0;
        }

        if (qs != null)
        {
            return qs.getProgress(infoNumber);
        }
        else
        {
            return "";
        }
    }

    public int getQuestProgressInt(int id)
    {
        if (int.TryParse(getQuestProgress(id), out var d))
            return d;
        return 0;
    }

    public int getQuestProgressInt(int id, int infoNumber)
    {
        if (int.TryParse(getQuestProgress(id, infoNumber), out var d))
            return d;
        return 0;
    }

    public void resetAllQuestProgress(int id)
    {
        QuestStatus qs = getPlayer().getQuest(Quest.getInstance(id));
        if (qs != null)
        {
            qs.resetAllProgress();
            getPlayer().announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
        }
    }

    public void resetQuestProgress(int id, int infoNumber)
    {
        QuestStatus qs = getPlayer().getQuest(Quest.getInstance(id));
        if (qs != null)
        {
            qs.resetProgress(infoNumber);
            getPlayer().announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
        }
    }

    public virtual bool forceStartQuest(int id)
    {
        return forceStartQuest(id, NpcId.MAPLE_ADMINISTRATOR);
    }

    public virtual bool forceStartQuest(int id, int npc)
    {
        return startQuest(id, npc);
    }

    public virtual bool forceCompleteQuest(int id)
    {
        return forceCompleteQuest(id, NpcId.MAPLE_ADMINISTRATOR);
    }

    public virtual bool forceCompleteQuest(int id, int npc)
    {
        return completeQuest(id, npc);
    }

    public virtual bool startQuest(short id)
    {
        return startQuest((int)id);
    }

    public virtual bool completeQuest(short id)
    {
        return completeQuest((int)id);
    }

    public virtual bool startQuest(int id)
    {
        return startQuest(id, NpcId.MAPLE_ADMINISTRATOR);
    }

    public virtual bool completeQuest(int id)
    {
        return completeQuest(id, NpcId.MAPLE_ADMINISTRATOR);
    }

    public virtual bool startQuest(short id, int npc)
    {
        return startQuest((int)id, npc);
    }

    public virtual bool completeQuest(short id, int npc)
    {
        return completeQuest((int)id, npc);
    }

    public virtual bool startQuest(int id, int npc)
    {
        try
        {
            return Quest.getInstance(id).forceStart(getPlayer(), npc);
        }
        catch (NullReferenceException ex)
        {
            Log.Logger.Error(ex.ToString());
            return false;
        }
    }

    public virtual bool completeQuest(int id, int npc)
    {
        try
        {
            return Quest.getInstance(id).forceComplete(getPlayer(), npc);
        }
        catch (NullReferenceException ex)
        {
            Log.Logger.Error(ex.ToString());
            return false;
        }
    }

    public Item? evolvePet(byte slot, int afterId)
    {
        Pet? evolved = null;
        Pet? target;

        long period = (long)TimeSpan.FromDays(90).TotalMilliseconds;    //refreshes expiration date: 90 days


        target = getPlayer().getPet(slot);
        if (target == null)
        {
            getPlayer().message("Pet could not be evolved...");
            return (null);
        }

        var tmp = gainItem(afterId, 1, false, true, period, target);

        /*
        evolved = Pet.loadFromDb(tmp.getItemId(), tmp.getPosition(), tmp.getPetId());

        evolved = tmp.getPet();
        if(evolved == null) {
            getPlayer().message("Pet structure non-existent for " + tmp.getItemId() + "...");
            return(null);
        }
        else if(tmp.getPetId() == -1) {
            getPlayer().message("Pet id -1");
            return(null);
        }

        getPlayer().addPet(evolved);

        getPlayer().getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.showPet(c.OnlinedCharacter, evolved, false, false), true);
        c.sendPacket(PacketCreator.petStatUpdate(c.OnlinedCharacter));
        c.sendPacket(PacketCreator.enableActions());
        chr.getClient().getWorldServer().registerPetHunger(chr, chr.getPetIndex(evolved));
        */

        InventoryManipulator.removeFromSlot(c, InventoryType.CASH, target.getPosition(), 1, false);

        return evolved;
    }
    // js使用
    public void gainItem(int id, bool show) => gainItem(id, 1, show);

    public void gainItem(int id, int quantity = 1, bool show = true)
    {
        //this will fk randomStats equip :P
        gainItem(id, (short)quantity, false, show);
    }

    public Item? gainItem(int id, short quantity, bool randomStats, bool showMessage, long expires = -1, Pet? from = null)
    {
        return getPlayer().GainItem(id, quantity, randomStats, showMessage, expires, from);
    }

    public void gainFame(int delta)
    {
        getPlayer().gainFame(delta);
    }

    public void changeMusic(string songName)
    {
        getPlayer().getMap().broadcastMessage(PacketCreator.musicChange(songName));
    }

    public void playerMessage(int type, string message)
    {
        getPlayer().dropMessage(type, message);
    }

    public void message(string message)
    {
        getPlayer().message(message);
    }

    public void dropMessage(int type, string message)
    {
        getPlayer().dropMessage(type, message);
    }

    public void mapMessage(int type, string message)
    {
        getPlayer().getMap().dropMessage(type, message);
    }

    public void mapEffect(string path)
    {
        c.sendPacket(PacketCreator.mapEffect(path));
    }

    public void mapSound(string path)
    {
        c.sendPacket(PacketCreator.mapSound(path));
    }

    public virtual void displayAranIntro()
    {
        string intro = (c.OnlinedCharacter.getMapId()) switch
        {
            MapId.ARAN_TUTO_1 => "Effect/Direction1.img/aranTutorial/Scene0",
            MapId.ARAN_TUTO_2 => "Effect/Direction1.img/aranTutorial/Scene1" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
            MapId.ARAN_TUTO_3 => "Effect/Direction1.img/aranTutorial/Scene2" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
            MapId.ARAN_TUTO_4 => "Effect/Direction1.img/aranTutorial/Scene3",
            MapId.ARAN_POLEARM => "Effect/Direction1.img/aranTutorial/HandedPoleArm" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
            MapId.ARAN_MAHA => "Effect/Direction1.img/aranTutorial/Maha",
            _ => ""
        };
        showIntro(intro);
    }

    public void showIntro(string path)
    {
        c.sendPacket(PacketCreator.showIntro(path));
    }

    public void showInfo(string path)
    {
        c.sendPacket(PacketCreator.showInfo(path));
        c.sendPacket(PacketCreator.enableActions());
    }

    public void guildMessage(int type, string message)
    {
        getGuild()?.dropMessage(type, message);
    }

    public Guild? getGuild()
    {
        try
        {
            return getPlayer().getGuild();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
        return null;
    }

    public virtual Team? getParty()
    {
        return getPlayer().getParty();
    }

    public bool isLeader()
    {
        return isPartyLeader();
    }

    public bool isGuildLeader()
    {
        return getPlayer().isGuildLeader();
    }

    public bool isPartyLeader()
    {
        if (getParty() == null)
        {
            return false;
        }

        return getParty()!.getLeaderId() == getPlayer().getId();
    }

    public bool isEventLeader()
    {
        return getEventInstance() != null && getPlayer().getId() == getEventInstance()!.getLeaderId();
    }

    public void giveCharacterExp(int amount, IPlayer chr)
    {
        chr.gainExp((int)(amount * chr.getExpRate()), true, true);
    }

    public void removeAll(int id)
    {
        removeAll(id, c);
    }

    public void removeAll(int id, IChannelClient cl)
    {
        InventoryType invType = ItemConstants.getInventoryType(id);
        int possessed = cl.OnlinedCharacter.getInventory(invType).countById(id);
        if (possessed > 0)
        {
            InventoryManipulator.removeById(cl, invType, id, possessed, true, false);
            cl.sendPacket(PacketCreator.getShowItemGain(id, (short)-possessed, true));
        }

        if (invType == InventoryType.EQUIP)
        {
            if (cl.OnlinedCharacter.getInventory(InventoryType.EQUIPPED).countById(id) > 0)
            {
                InventoryManipulator.removeById(cl, InventoryType.EQUIPPED, id, 1, true, false);
                cl.sendPacket(PacketCreator.getShowItemGain(id, -1, true));
            }
        }
    }

    public virtual int getMapId()
    {
        return c.OnlinedCharacter.getMap().getId();
    }

    public int getPlayerCount(int mapid)
    {
        return c.CurrentServer.getMapFactory().getMap(mapid).getAllPlayers().Count;
    }

    public void showInstruction(string msg, int width, int height)
    {
        c.sendPacket(PacketCreator.sendHint(msg, width, height));
        c.sendPacket(PacketCreator.enableActions());
    }

    public void disableMinimap()
    {
        c.sendPacket(PacketCreator.disableMinimap());
    }

    public bool isAllReactorState(int reactorId, int state)
    {
        return c.OnlinedCharacter.getMap().isAllReactorState(reactorId, state);
    }

    public virtual void resetMap(int mapid)
    {
        getMap(mapid).clearMapObjects();
    }

    public void useItem(int id)
    {
        ItemInformationProvider.getInstance().GetItemEffectTrust(id).applyTo(c.OnlinedCharacter);
        c.sendPacket(PacketCreator.getItemMessage(id));//Useful shet :3
    }

    public void cancelItem(int id)
    {
        getPlayer().cancelEffect(ItemInformationProvider.getInstance().GetItemEffectTrust(id), false, -1);
    }

    public void teachSkill(int skillid, sbyte level, sbyte masterLevel, long expiration, bool force = false)
    {
        var skill = SkillFactory.GetSkillTrust(skillid);
        var skillEntry = getPlayer().getSkills().GetValueOrDefault(skill);
        if (skillEntry != null)
        {
            if (!force && level > -1)
            {
                getPlayer().changeSkillLevel(skill,
                    Math.Max(skillEntry.skillevel, level),
                    Math.Max(skillEntry.masterlevel, masterLevel),
                    expiration == -1 ? -1 : Math.Max(skillEntry.expiration, expiration));
                return;
            }
        }
        else if (GameConstants.isAranSkills(skillid))
        {
            c.sendPacket(PacketCreator.showInfo("Effect/BasicEff.img/AranGetSkill"));
        }

        getPlayer().changeSkillLevel(skill, level, masterLevel, expiration);
    }

    public void removeEquipFromSlot(short slot)
    {
        var tempItem = c.OnlinedCharacter.getInventory(InventoryType.EQUIPPED).getItem(slot);
        if (tempItem == null)
            return;

        InventoryManipulator.removeFromSlot(c, InventoryType.EQUIPPED, slot, tempItem.getQuantity(), false, false);
    }

    public void gainAndEquip(int itemid, short slot)
    {
        var old = c.OnlinedCharacter.getInventory(InventoryType.EQUIPPED).getItem(slot);
        if (old != null)
        {
            InventoryManipulator.removeFromSlot(c, InventoryType.EQUIPPED, slot, old.getQuantity(), false, false);
        }
        Item newItem = ItemInformationProvider.getInstance().getEquipById(itemid);
        newItem.setPosition(slot);
        c.OnlinedCharacter.getInventory(InventoryType.EQUIPPED).addItemFromDB(newItem);
        c.sendPacket(PacketCreator.modifyInventory(false, Collections.singletonList(new ModifyInventory(0, newItem))));
    }

    public void spawnNpc(int npcId, Point pos, IMap map)
    {
        var npc = LifeFactory.Instance.getNPC(npcId);
        if (npc != null)
        {
            npc.setPosition(pos);
            npc.setCy(pos.Y);
            npc.setRx0(pos.X + 50);
            npc.setRx1(pos.X - 50);
            npc.setFh(map.Footholds.FindBelowFoothold(pos)!.getId());
            map.addMapObject(npc);
            map.broadcastMessage(PacketCreator.spawnNPC(npc));
        }
    }

    public void spawnMonster(int id, int x, int y)
    {
        var monster = LifeFactory.Instance.GetMonsterTrust(id);
        monster.setPosition(new Point(x, y));
        getPlayer().getMap().spawnMonster(monster);
    }

    public Monster? getMonsterLifeFactory(int mid)
    {
        return LifeFactory.Instance.getMonster(mid);
    }

    public void spawnGuide()
    {
        c.sendPacket(PacketCreator.spawnGuide(true));
    }

    public void removeGuide()
    {
        c.sendPacket(PacketCreator.spawnGuide(false));
    }

    public void displayGuide(int num)
    {
        c.sendPacket(PacketCreator.showInfo("UI/tutorial.img/" + num));
    }

    public void goDojoUp()
    {
        c.sendPacket(PacketCreator.dojoWarpUp());
    }

    public void resetDojoEnergy()
    {
        c.OnlinedCharacter.setDojoEnergy(0);
    }

    public void resetPartyDojoEnergy()
    {
        foreach (var pchr in c.OnlinedCharacter.getPartyMembersOnSameMap())
        {
            pchr.setDojoEnergy(0);
        }
    }

    public void enableActions()
    {
        c.sendPacket(PacketCreator.enableActions());
    }

    public virtual void showEffect(string effect)
    {
        c.sendPacket(PacketCreator.showEffect(effect));
    }

    public void dojoEnergy()
    {
        c.sendPacket(PacketCreator.getEnergy("energy", getPlayer().getDojoEnergy()));
    }

    public void talkGuide(string message)
    {
        c.sendPacket(PacketCreator.talkGuide(message));
    }

    public void guideHint(int hint)
    {
        c.sendPacket(PacketCreator.guideHint(hint));
    }

    public void updateAreaInfo(short area, string info)
    {
        c.OnlinedCharacter.updateAreaInfo(area, info);
        c.sendPacket(PacketCreator.enableActions());//idk, nexon does the same :P
    }

    public bool containsAreaInfo(short area, string info)
    {
        return c.OnlinedCharacter.containsAreaInfo(area, info);
    }

    public void earnTitle(string msg)
    {
        c.sendPacket(PacketCreator.earnTitleMessage(msg));
    }

    public void showInfoText(string msg)
    {
        c.sendPacket(PacketCreator.showInfoText(msg));
    }

    public void openUI(byte ui)
    {
        c.sendPacket(PacketCreator.openUI(ui));
    }

    public void lockUI()
    {
        c.sendPacket(PacketCreator.disableUI(true));
        c.sendPacket(PacketCreator.lockUI(true));
    }

    public void unlockUI()
    {
        c.sendPacket(PacketCreator.disableUI(false));
        c.sendPacket(PacketCreator.lockUI(false));
    }

    public void playSound(string sound)
    {
        getPlayer().getMap().broadcastMessage(PacketCreator.environmentChange(sound, 4));
    }

    public void environmentChange(string env, int mode)
    {
        getPlayer().getMap().broadcastMessage(PacketCreator.environmentChange(env, mode));
    }

    public string numberWithCommas(int number)
    {
        return GameConstants.numberWithCommas(number);
    }

    public Pyramid? getPyramid()
    {
        return getPlayer().getPartyQuest() as Pyramid;
    }

    public int createExpedition(ExpeditionType type, bool silent = false, int minPlayers = 0, int maxPlayers = 0)
    {
        var player = getPlayer();
        Expedition exped = new Expedition(player, type, silent, minPlayers, maxPlayers);

        var channelServer = player.getMap().getChannelServer();
        int channel = channelServer.getId();
        if (!channelServer.Container.ExpeditionService.CanStartExpedition(player.getId(), channel, exped.getType().name()))
        {
            // thanks Conrad for noticing missing expeditions entry limit
            return 1;
        }

        if (exped.addChannelExpedition(player.getClient().getChannelServer()))
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public void endExpedition(Expedition exped)
    {
        exped.dispose(true);
        exped.removeChannelExpedition(getPlayer().getClient().getChannelServer());
    }

    public Expedition? getExpedition(ExpeditionType type)
    {
        return getPlayer().getClient().getChannelServer().getExpedition(type);
    }

    public string getExpeditionMemberNames(ExpeditionType type)
    {
        string members = "";
        var exped = getExpedition(type);
        if (exped != null)
        {
            foreach (string memberName in exped.getMembers().Values)
            {
                members += "" + memberName + ", ";
            }
        }
        return members;
    }

    public bool isLeaderExpedition(ExpeditionType type)
    {
        return getExpedition(type)?.isLeader(getPlayer()) ?? false;
    }

    public long getJailTimeLeft()
    {
        return getPlayer().getJailExpirationTimeLeft();
    }

    public List<Pet> getDriedPets()
    {
        List<Pet> list = new();

        long curTime = c.CurrentServerContainer.getCurrentTime();
        foreach (Item it in getPlayer().getInventory(InventoryType.CASH).list())
        {
            if (it is Pet pet && pet.getExpiration() < curTime)
            {
                if (pet != null)
                {
                    list.Add(pet);
                }
            }
        }

        return list;
    }

    public bool startDungeonInstance(int dungeonid)
    {
        return c.CurrentServer.addMiniDungeon(dungeonid);
    }

    public bool canGetFirstJob(int jobType)
    {
        if (YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP)
        {
            return true;
        }

        var chr = this.getPlayer();

        return jobType switch
        {
            1 => chr.getStr() >= 35,
            2 => chr.getInt() >= 20,
            3 or 4 => chr.getDex() >= 25,
            5 => chr.getDex() >= 20,
            _ => true,
        };
    }

    public string? getFirstJobStatRequirement(int jobType)
    {
        return jobType switch
        {
            1 => "STR " + 35,
            2 => "INT " + 20,
            3 or 4 => "DEX " + 25,
            5 => "DEX " + 20,
            _ => null,
        };
    }

    public void npcTalk(int npcid, string message)
    {
        c.sendPacket(PacketCreator.getNPCTalk(npcid, 0, message, "00 00", 0));
    }

    public long getCurrentTime()
    {
        return c.CurrentServerContainer.getCurrentTime();
    }

    public void weakenAreaBoss(int monsterId, string message)
    {
        var map = c.OnlinedCharacter.getMap();
        var monster = map.getMonsterById(monsterId);
        if (monster == null)
        {
            return;
        }

        applySealSkill(monster);
        applyReduceAvoid(monster);
        map.dropMessage(6, message);
    }

    private void applySealSkill(Monster monster)
    {
        MobSkill sealSkill = MobSkillFactory.getMobSkillOrThrow(MobSkillType.SEAL_SKILL, 1);
        sealSkill.applyEffect(monster);
    }

    private void applyReduceAvoid(Monster monster)
    {
        MobSkill reduceAvoidSkill = MobSkillFactory.getMobSkillOrThrow(MobSkillType.EVA, 2);
        reduceAvoidSkill.applyEffect(monster);
    }

    public void LearnExtraSkill(int skillId)
    {
        var skill = SkillFactory.GetSkillTrust(skillId);
        getPlayer().changeSkillLevel(skill, (sbyte)skill.getMaxLevel(), skill.getMaxLevel(), -1);
        getPlayer().changeKeybinding((int)KeyCode.Equal, new KeyBinding(KeyBindingType.Skill, skillId));
        getPlayer().sendKeymap();
    }

    public void TypedMessage(int type, string messageKey, params string[] param)
    {
        getPlayer().TypedMessage(type, messageKey, param);
    }

    public void Dialog(string key, params string[] param)
    {
        getPlayer().Dialog(key, param);
    }

    public void Yellow(string key, params string[] param)
    {
        getPlayer().Yellow(key, param);
    }

    public void Notice(string key, params string[] param)
    {
        TypedMessage(0, key, param);
    }

    public void Popup(string key, params string[] param)
    {
        TypedMessage(1, key, param);
    }

    public void Pink(string key, params string[] param)
    {
        TypedMessage(5, key, param);
    }

    public void LightBlue(string key, params string[] param)
    {
        TypedMessage(6, key, param);
    }

    public void LightBlue(Func<ClientCulture, string> action)
    {
        getPlayer().LightBlue(action);
    }

    public void TopScrolling(string key, params string[] param)
    {
        getPlayer().TopScrolling(key, param);
    }

}