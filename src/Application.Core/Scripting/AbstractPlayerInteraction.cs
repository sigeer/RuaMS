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
using Application.Core.Client.inventory;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Infrastructure;
using Application.Core.Scripting.Events;
using Application.Shared.Events;
using client;
using client.inventory;
using client.inventory.manipulator;
using server.expeditions;
using server.life;
using server.partyquest;
using server.quest;
using System.Threading.Tasks;
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

    public Player getPlayer()
    {
        return c.OnlinedCharacter;
    }

    public Player getChar()
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

    public string GetJobName(int job)
    {
        return getClient().CurrentCulture.GetJobName(JobFactory.GetById(job));
    }

    public string Ordinal(int i)
    {
        return getClient().CurrentCulture.Ordinal(i);
    }

    public int getLevel()
    {
        return getPlayer().getLevel();
    }

    public virtual IMap getMap()
    {
        return c.OnlinedCharacter.getMap();
    }
    public virtual int getMapId()
    {
        return c.OnlinedCharacter.getMap().getId();
    }

    public int getHourOfDay()
    {
        return c.CurrentServer.Node.GetCurrentTimeDateTimeOffset().ToLocalTime().Hour;
    }

    public async Task<int> getMarketPortalId(int mapId)
    {
        return getMarketPortalId(await getWarpMap(mapId));
    }

    private int getMarketPortalId(IMap map)
    {
        return (map.findMarketPortal() != null) ? map.findMarketPortal()!.getId() : map.getRandomPlayerSpawnpoint().getId();
    }

    public async Task WarpOut()
    {
        await warp(getPlayer().getMap().getForcedReturnId());
    }

    public async Task WarpReturn()
    {
        await warp(getPlayer().getMap().getReturnMapId());
    }

    public async Task WarpReturn(int portal)
    {
        await warp(getPlayer().getMap().getReturnMapId(), portal);
    }

    public async Task warp(int mapid)
    {
        await getPlayer().changeMap(mapid);
    }

    public async Task warp(int map, int portal)
    {
        await getPlayer().changeMap(map, portal);
    }

    public async Task warp(int map, string portal)
    {
        await getPlayer().changeMap(map, portal);
    }

    public void warpMap(int map)
    {
        getPlayer().getMap().warpEveryone(map);
    }

    public async Task warpParty(int id)
    {
        await warpParty(id, 0);
    }

    public async Task warpParty(int id, int portalId)
    {
        int mapid = getMapId();
        await warpParty(id, portalId, mapid, mapid);
    }

    public async Task warpParty(int map, string portalName)
    {

        int mapid = getMapId();
        var warpMap = await c.CurrentServer.getMapFactory().getMap(map);

        var portal = warpMap.getPortal(portalName);

        if (portal == null)
        {
            portal = warpMap.getPortal(0)!;
        }

        var portalId = portal.getId();

        await warpParty(map, portalId, mapid, mapid);

    }

    public async Task warpParty(int id, int fromMinId, int fromMaxId)
    {
        await warpParty(id, 0, fromMinId, fromMaxId);
    }

    public async Task warpParty(int id, int portalId, int fromMinId, int fromMaxId)
    {
        foreach (var mc in this.getPlayer().getPartyMembersOnline())
        {
            if (mc.isLoggedinWorld())
            {
                if (mc.getMapId() >= fromMinId && mc.getMapId() <= fromMaxId)
                {
                    await mc.changeMap(id, portalId);
                }
            }
        }
    }

    public Task TeleportPortal(int portalId)
    {
        return c.SendPacket(PacketCreator.TeleportPortal(false, portalId));
    }

    public async Task<IMap> getWarpMap(int map)
    {
        return await getPlayer().getWarpMap(map);
    }

    public async Task<IMap> getMap(int map)
    {
        return await getWarpMap(map);
    }

    public async Task<int> countAllMonstersOnMap(int map)
    {
        return (await getMap(map)).countMonsters();
    }

    public int countMonster()
    {
        return getPlayer().getMap().countMonsters();
    }

    public async Task resetMapObjects(int mapid)
    {
        await (await getWarpMap(mapid)).resetMapObjects();
    }

    public AbstractEventManager? getEventManager(string @event)
    {
        return getPlayer().getEventManager(@event);
    }

    public AbstractEventManager GetEventManager(string @event) => getEventManager(@event) ?? throw new BusinessException($"Error: 事件 {@event} 未注册");

    public TEventManager GetEventManager<TEventManager>(string @event) where TEventManager : AbstractEventManager
    {
        var em = getPlayer().getEventManager(@event);
        if (em == null)
        {
            throw new BusinessNotsupportException($"Event: {@event}");
        }

        return (em as TEventManager) ?? throw new BusinessException($"Error: {@event} 不是 {typeof(TEventManager).Name}");
    }

    public SoloEventManager GetSoloQuestEventManager(int questId)
    {
        var @event = $"q{questId}";
        var em = getPlayer().getEventManager(@event);
        if (em == null)
        {
            throw new BusinessNotsupportException($"Event: {@event}");
        }

        return (em as SoloEventManager) ?? throw new BusinessException($"Error: {@event} 不是 SoloEventManager");
    }

    public AbstractEventInstanceManager? getEventInstance()
    {
        return getPlayer().getEventInstance();
    }

    public AbstractEventInstanceManager GetEventInstanceTrust()
    {
        return getPlayer().getEventInstance() ?? throw new ConversationDiffInstanceException();
    }

    public TEim GetEventInstanceTrust<TEim>() where TEim : AbstractEventInstanceManager
    {
        return (getPlayer().getEventInstance() as TEim) ?? throw new ConversationDiffInstanceException();
    }

    public AbstractInventory getInventory(int type)
    {
        return getPlayer().getInventory(InventoryTypeUtils.getByType((sbyte)type));
    }

    public AbstractInventory getInventory(InventoryType type)
    {
        return getPlayer().getInventory(type);
    }

    public bool hasItem(int itemid, int quantity = 1)
    {
        return haveItem(itemid, quantity);
    }


    public bool haveItem(int itemid, int quantity = 1)
    {
        return getItemQuantity(itemid) >= quantity;
    }

    [ScriptCall]
    public int getItemQuantity(int itemid)
    {
        return getPlayer().getItemQuantity(itemid);
    }

    [ScriptCall]
    public bool haveItemWithId(int itemid, bool checkEquipped = false)
    {
        return getPlayer().haveItemWithId(itemid, checkEquipped);
    }


    public bool canHold(int itemid, int quantity = 1)
    {
        return canHoldAll(Collections.singletonList(itemid), Collections.singletonList(quantity), true);
    }

    public async Task<bool> canHold(int itemid, int quantity, int removeItemid, int removeQuantity)
    {
        return await canHoldAllAfterRemoving(Collections.singletonList(itemid), Collections.singletonList(quantity), Collections.singletonList(removeItemid), Collections.singletonList(removeQuantity));
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

        List<ItemQuantity> addedItems = new();
        for (int i = 0; i < size; i++)
        {
            addedItems.Add(new ItemQuantity(itemids.get(i), (short)quantity.ElementAtOrDefault(i)));
        }

        return Inventory.checkSpots(c.OnlinedCharacter, addedItems);
    }

    public bool CanHoldAll(IEnumerable<ItemQuantity> items)
    {
        return Inventory.checkSpots(c.OnlinedCharacter, items);
    }


    private List<TypedItemQuantity> prepareProofInventoryItems(List<ItemQuantity> items)
    {
        List<TypedItemQuantity> addedItems = new();
        foreach (var p in items)
        {
            Item it = new Item(p.ItemId, 0, (short)p.Quantity);
            addedItems.Add(new((sbyte)InventoryType.CANHOLD, new(p.ItemId, p.Quantity)));
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

    public async Task<bool> canHoldAllAfterRemoving(List<int> toAddItemids, List<int> toAddQuantity, List<int> toRemoveItemids, List<int> toRemoveQuantity)
    {
        List<List<ItemQuantity>> toAddItemList = prepareInventoryItemList(toAddItemids, toAddQuantity);
        List<List<ItemQuantity>> toRemoveItemList = prepareInventoryItemList(toRemoveItemids, toRemoveQuantity);

        InventoryProof prfInv = (InventoryProof)this.getInventory(InventoryType.CANHOLD);

        try
        {
            for (int i = InventoryType.EQUIP.getType(); i < InventoryType.CASH.getType(); i++)
            {
                List<ItemQuantity> toAdd = toAddItemList.get(i);

                if (toAdd.Count > 0)
                {
                    List<ItemQuantity> toRemove = toRemoveItemList.get(i);

                    var inv = this.getInventory(i) as Inventory;
                    prfInv.cloneContents(inv);

                    foreach (var p in toRemove)
                    {
                        await InventoryManipulator.removeById(c, InventoryType.CANHOLD, p.ItemId, p.Quantity, false, false);
                    }

                    List<TypedItemQuantity> addItems = prepareProofInventoryItems(toAdd);

                    bool canHold = Inventory.checkSpots(c.OnlinedCharacter, addItems);
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

    public async Task openNpc(int npcid, string? script = null)
    {
        await c.OnlinedCharacter.OpenNpc(npcid, script);
    }

    public int getQuestStatus(int id)
    {
        return (int)getQuestStat(id);
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

    /// <summary>
    /// 进行中
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    public bool IsQuestNotStarted(int id)
    {
        try
        {
            return getQuestStat(id) == QuestStatus.Status.NOT_STARTED;
        }
        catch (NullReferenceException e)
        {
            Log.Logger.Error(e.ToString());
            return false;
        }
    }

    public async Task setQuestProgress(int id, string progress)
    {
        await c.OnlinedCharacter.SetQuestProgress(id, progress);
    }

    public async Task setQuestProgress(int id, int progress)
    {
        await c.OnlinedCharacter.SetQuestProgress(id, progress);
    }

    public async Task setQuestProgress(int id, int infoNumber, int progress)
    {
        await c.OnlinedCharacter.SetQuestProgress(id, infoNumber, progress);
    }

    public async Task setQuestProgress(int id, int infoNumber, string progress)
    {
        await c.OnlinedCharacter.setQuestProgress(id, infoNumber, progress);
    }

    public string getQuestProgress(int id, int infoNumber = 0)
    {
        return getPlayer().GetQuestProgress(id, infoNumber);
    }
    public int getQuestProgressInt(int id, int infoNumber = 0) => getPlayer().GetQuestProgressInt(id, infoNumber);

    public async Task resetAllQuestProgress(int id)
    {
        QuestStatus qs = getPlayer().getQuest(Quest.getInstance(id));
        if (qs != null)
        {
            qs.resetAllProgress();
            await getPlayer().announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
        }
    }

    public async Task resetQuestProgress(int id, int infoNumber)
    {
        QuestStatus qs = getPlayer().getQuest(Quest.getInstance(id));
        if (qs != null)
        {
            qs.resetProgress(infoNumber);
            await getPlayer().announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
        }
    }


    public Task<bool> forceStartQuest(int id, int npc = NpcId.MAPLE_ADMINISTRATOR)
    {
        return getPlayer().ForceStartQuest(id, npc);
    }


    public Task<bool> forceCompleteQuest(int id, int npc = NpcId.MAPLE_ADMINISTRATOR)
    {
        return getPlayer().ForceCompleteQuest(id, npc);
    }

    public Task<bool> startQuest(int id, int npc = NpcId.MAPLE_ADMINISTRATOR)
    {
        return forceStartQuest(id, npc);
    }

    public Task<bool> completeQuest(int id, int npc = NpcId.MAPLE_ADMINISTRATOR)
    {
        return forceCompleteQuest(id, npc);
    }

    public async Task<Item?> evolvePet(sbyte slot)
    {
        var target = getPlayer().getPet(slot);
        if (target != null)
        {
            var pet = target.PetItem.EvolvePet(getPlayer());
            if (pet != null)
            {
                await InventoryManipulator.removeFromSlot(c, InventoryType.CASH, target.PetItem.getPosition(), 1, false);

                await InventoryManipulator.addFromDrop(getClient(), pet, false);
                await getPlayer().SummonPet(pet);
                return pet;
            }
        }

        await getPlayer().Pink("Pet could not be evolved...");
        return null;

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
        await c.SendPacket(PacketCreator.petStatUpdate(c.OnlinedCharacter));
        await c.SendPacket(PacketCreator.enableActions());
        chr.getClient().getWorldServer().registerPetHunger(chr, chr.getPetIndex(evolved));
        */


    }
    // js使用
    [ScriptCall]
    public Task gainItem(int id, bool show) => gainItem(id, 1, show);

    public async Task gainItem(int id, int quantity = 1, bool show = true)
    {
        //this will fk randomStats equip :P
        await gainItem(id, (short)quantity, false, show);
    }

    public async Task<Item?> gainItem(int id, short quantity, bool randomStats, bool showMessage, long expires = -1)
    {
        return await getPlayer().GainItem(id, quantity, randomStats,
            showMessage ? GainItemShow.ShowInChat : GainItemShow.NotShown, expires: expires);
    }

    public async Task gainFame(int delta)
    {
        await getPlayer().gainFame(delta);
    }

    public async Task changeMusic(string songName)
    {
        await getPlayer().getMap().broadcastMessage(PacketCreator.musicChange(songName));
    }

    public async Task playerMessage(int type, string message)
    {
        await getPlayer().dropMessage(type, message);
    }

    public async Task message(string message)
    {
        await Pink(message);
    }

    public async Task dropMessage(int type, string message)
    {
        await TypedMessage(type, message);
    }

    public async Task mapMessage(int type, string message)
    {
        await getPlayer().getMap().TypedMessage(type, message);
    }

    public async Task mapEffect(string path)
    {
        await c.SendPacket(PacketCreator.mapEffect(path));
    }

    public async Task mapSound(string path)
    {
        await c.SendPacket(PacketCreator.mapSound(path));
    }

    public virtual async Task displayAranIntro()
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
        await showIntro(intro);
    }

    public async Task showIntro(string path)
    {
        await c.SendPacket(PacketCreator.showIntro(path));
    }

    public async Task showInfo(string path)
    {
        await c.SendPacket(PacketCreator.showInfo(path));
        await c.SendPacket(PacketCreator.enableActions());
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

    public async Task giveCharacterExp(int amount, Player chr)
    {
        await chr.gainExp((int)(amount * chr.getExpRate()), true, true);
    }

    public async Task removeAll(int id)
    {
        await removeAll(id, c);
    }

    public async Task removeAll(int id, IChannelClient cl)
    {
        InventoryType invType = ItemConstants.getInventoryType(id);
        int possessed = cl.OnlinedCharacter.getInventory(invType).countById(id);
        if (possessed > 0)
        {
            await InventoryManipulator.removeById(cl, invType, id, possessed, true, false);
            await cl.SendPacket(PacketCreator.getShowItemGain(id, (short)-possessed, true));
        }

        if (invType == InventoryType.EQUIP)
        {
            if (cl.OnlinedCharacter.getInventory(InventoryType.EQUIPPED).countById(id) > 0)
            {
                await InventoryManipulator.removeById(cl, InventoryType.EQUIPPED, id, 1, true, false);
                await cl.SendPacket(PacketCreator.getShowItemGain(id, -1, true));
            }
        }
    }

    public async Task RemoveFirstSlot(InventoryType type)
    {
        await InventoryManipulator.removeFromSlot(getClient(), type, 1, 1, true);
    }


    public async Task<int> getPlayerCount(int mapid)
    {
        return (await c.CurrentServer.getMapFactory().getMap(mapid)).getAllPlayers().Count;
    }

    public Task showInstruction(string msg, int width, int height)
    {
        return getPlayer().announceHint(msg, width, height);
    }

    public async Task disableMinimap()
    {
        await c.SendPacket(PacketCreator.disableMinimap());
    }

    public bool isAllReactorState(int reactorId, int state)
    {
        return c.OnlinedCharacter.getMap().isAllReactorState(reactorId, state);
    }

    public virtual async Task resetMap(int mapid)
    {
        await (await getMap(mapid)).clearMapObjects();
    }

    public async Task useItem(int id)
    {
        await ItemInformationProvider.getInstance().GetItemEffectTrust(id).applyTo(c.OnlinedCharacter);
        await c.SendPacket(PacketCreator.getItemMessage(id));//Useful shet :3
    }

    public async Task cancelItem(int id)
    {
        await getPlayer().cancelEffect(ItemInformationProvider.getInstance().GetItemEffectTrust(id), false);
    }

    public async Task teachSkill(int skillid, sbyte level, sbyte masterLevel, long expiration, bool force = false)
    {
        var skill = SkillFactory.GetSkillTrust(skillid);
        var skillEntry = getPlayer().getSkills().GetValueOrDefault(skill);
        if (skillEntry != null)
        {
            if (!force && level > -1)
            {
                await getPlayer().changeSkillLevel(skill,
                     Math.Max(skillEntry.skillevel, level),
                     Math.Max(skillEntry.masterlevel, masterLevel),
                     expiration == -1 ? -1 : Math.Max(skillEntry.expiration, expiration));
                return;
            }
        }
        else if (GameConstants.isAranSkills(skillid))
        {
            await c.SendPacket(PacketCreator.showInfo("Effect/BasicEff.img/AranGetSkill"));
        }

        await getPlayer().changeSkillLevel(skill, level, masterLevel, expiration);
    }

    public async Task removeEquipFromSlot(short slot)
    {
        var tempItem = c.OnlinedCharacter.getInventory(InventoryType.EQUIPPED).getItem(slot);
        if (tempItem == null)
            return;

        await InventoryManipulator.removeFromSlot(c, InventoryType.EQUIPPED, slot, tempItem.getQuantity(), false, false);
    }


    public async Task spawnNpc(int npcId, Point pos, IMap map)
    {
        await map.SpawnNpc(npcId, pos);
    }

    public async Task spawnMonster(int id, int x, int y)
    {
        await getPlayer().getMap().spawnMonsterOnGroundBelow(id, x, y);
    }


    public async Task spawnGuide()
    {
        await c.SendPacket(PacketCreator.spawnGuide(true));
    }

    public async Task removeGuide()
    {
        await c.SendPacket(PacketCreator.spawnGuide(false));
    }

    public async Task displayGuide(int num)
    {
        await c.SendPacket(PacketCreator.showInfo("UI/tutorial.img/" + num));
    }

    public async Task goDojoUp()
    {
        await c.SendPacket(PacketCreator.dojoWarpUp());
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

    public async Task enableActions()
    {
        await c.SendPacket(PacketCreator.enableActions());
    }

    public virtual async Task showEffect(string effect)
    {
        await c.SendPacket(PacketCreator.showEffect(effect));
    }

    public async Task dojoEnergy()
    {
        await c.SendPacket(PacketCreator.getEnergy("energy", getPlayer().getDojoEnergy()));
    }

    public async Task talkGuide(string message)
    {
        await c.SendPacket(PacketCreator.talkGuide(message));
    }

    public async Task guideHint(int hint)
    {
        await c.SendPacket(PacketCreator.guideHint(hint));
    }

    public async Task updateAreaInfo(short area, string info)
    {
        await c.OnlinedCharacter.updateAreaInfo(area, info);
        await c.SendPacket(PacketCreator.enableActions());//idk, nexon does the same :P
    }

    public bool containsAreaInfo(short area, string info)
    {
        return c.OnlinedCharacter.containsAreaInfo(area, info);
    }

    public async Task earnTitle(string msg)
    {
        await c.SendPacket(PacketCreator.earnTitleMessage(msg));
    }

    public async Task showInfoText(string msg)
    {
        await c.SendPacket(PacketCreator.showInfoText(msg));
    }

    public async Task openUI(byte ui)
    {
        await c.SendPacket(PacketCreator.openUI(ui));
    }

    public async Task lockUI()
    {
        await c.SendPacket(PacketCreator.disableUI(true));
        await c.SendPacket(PacketCreator.lockUI(true));
    }

    public async Task unlockUI()
    {
        await c.SendPacket(PacketCreator.disableUI(false));
        await c.SendPacket(PacketCreator.lockUI(false));
    }

    public async Task playSound(string sound)
    {
        await getPlayer().getMap().broadcastMessage(PacketCreator.environmentChange(sound, 4));
    }

    public async Task environmentChange(string env, int mode)
    {
        await getPlayer().getMap().broadcastMessage(PacketCreator.environmentChange(env, mode));
    }

    public string numberWithCommas(int number)
    {
        return getClient().CurrentCulture.Number(number);
    }

    public Pyramid? getPyramid()
    {
        return getPlayer().getPartyQuest() as Pyramid;
    }

    public async Task<int> createExpedition(ExpeditionType type, bool silent = false, int minPlayers = 0, int maxPlayers = 0)
    {
        var player = getPlayer();
        Expedition exped = new Expedition(player, type, silent, minPlayers, maxPlayers);

        var channelServer = player.getMap().getChannelServer();
        int channel = channelServer.getId();
        if (!channelServer.NodeService.ExpeditionService.CanStartExpedition(player.getId(), channel, exped.getType().name()))
        {
            // thanks Conrad for noticing missing expeditions entry limit
            return 1;
        }

        if (await exped.addChannelExpedition(player.getClient().getChannelServer()))
        {
            return 0;
        }
        else
        {
            return -1;
        }
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

        long curTime = c.CurrentServer.Node.getCurrentTime();
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

    public async Task<bool> startDungeonInstance(int dungeonid)
    {
        return await c.CurrentServer.addMiniDungeon(dungeonid);
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

    public long getCurrentTime()
    {
        return c.CurrentServer.Node.getCurrentTime();
    }

    public async Task weakenAreaBoss(int monsterId, string message)
    {
        var map = c.OnlinedCharacter.getMap();
        var monster = map.getMonsterById(monsterId);
        if (monster == null)
        {
            return;
        }

        await applySealSkill(monster);
        await applyReduceAvoid(monster);
        await map.LightBlue(message);
    }

    private async Task applySealSkill(Monster monster)
    {
        MobSkill sealSkill = MobSkillFactory.getMobSkillOrThrow(MobSkillType.SEAL_SKILL, 1);
        await sealSkill.applyEffect(monster);
    }

    private async Task applyReduceAvoid(Monster monster)
    {
        MobSkill reduceAvoidSkill = MobSkillFactory.getMobSkillOrThrow(MobSkillType.EVA, 2);
        await reduceAvoidSkill.applyEffect(monster);
    }

    public async Task LearnExtraSkill(int skillId)
    {
        await getPlayer().LearnSkill(skillId);
    }

    public Task TypedMessage(int type, string messageKey, params string[] param)
    {
        return getPlayer().TypedMessage(type, messageKey, param);
    }

    public Task Notice(string key, params string[] param) => TypedMessage(0, key, param);

    public Task Popup(string key, params string[] param) => TypedMessage(1, key, param);

    public Task TopScrolling(string key, params string[] param) => TypedMessage(4, key, param);

    public Task Pink(string key, params string[] param) => TypedMessage(5, key, param);

    public Task LightBlue(string key, params string[] param) => TypedMessage(6, key, param);

    public Task Yellow(string key, params string[] param) => TypedMessage(-1, key, param);
    public Task EarnTitle(string key, params string[] param) => TypedMessage(-2, key, param);
    public Task Dialog(string key, params string[] param) => TypedMessage(-3, key, param);

    public Task LightBlue(Func<ClientCulture, string> action)
    {
        return getPlayer().LightBlue(action);
    }

    #region Quest
    public async Task touchTheSky()
    { //29004
        Quest quest = Quest.getInstance(29004);
        if (!isQuestStarted(29004))
        {
            if (!await quest.forceStart(getPlayer(), 9000066))
            {
                return;
            }
        }
        QuestStatus qs = getPlayer().getQuest(quest);
        if (!qs.addMedalMap(getPlayer().getMapId()))
        {
            return;
        }
        string status = qs.getMedalProgress().ToString();
        await getPlayer().announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
        await getPlayer().SendPacket(PacketCreator.earnTitleMessage(status + "/5 已完成"));
        await getPlayer().SendPacket(PacketCreator.earnTitleMessage("站在巅峰的人 勋章挑战正在进行中"));
        if (qs.getMedalProgress().ToString() == qs.getInfoEx(0))
        {
            await showInfoText("T站在巅峰的人 勋章挑战正在进行中。 勋章挑战已完成！请找勋章老人领取你的勋章。");
            await getPlayer().SendPacket(PacketCreator.getShowQuestCompletion(quest.getId()));
        }
        else
        {
            await showInfoText("站在巅峰的人 勋章挑战正在进行中。 " + status + "/5 已完成");
        }
    }
    #endregion

    #region Guild
    public Task GainGuildGP(int value)
    {
        return c.CurrentServer.NodeService.GuildManager.GainGP(getPlayer(), value);
    }
    #endregion

    public ContiMoveBase? GetContiMove()
    {
        return c.CurrentServer.ContiMoves.Values.Where(x => x.StationAMap == c.OnlinedCharacter.MapModel || x.StationBMap == c.OnlinedCharacter.MapModel).FirstOrDefault();
    }
}