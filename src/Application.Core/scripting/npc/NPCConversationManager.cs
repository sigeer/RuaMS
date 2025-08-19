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
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Models;
using Application.Core.scripting.Infrastructure;
using Application.Resources;
using Application.Shared.Events;
using constants.game;
using constants.String;
using Microsoft.Extensions.DependencyInjection;
using net.server.coordinator.matchchecker;
using server;
using server.expeditions;
using server.life;
using server.partyquest;
using tools;
using static server.partyquest.Pyramid;


namespace scripting.npc;

/**
 * @author Matze
 */
public class NPCConversationManager : AbstractPlayerInteraction
{
    private ILogger log = LogFactory.GetLogger(LogType.Conversation);

    private int npc;
    private int npcOid;
    public ScriptMeta ScriptMeta { get; }
    private string? _getText;
    private bool itemScript;
    private List<IPlayer> otherParty;

    private Dictionary<int, string> npcDefaultTalks = new();
    public NextLevelContext NextLevelContext { get; set; } = new NextLevelContext();

    private string getDefaultTalk(int npcid)
    {
        var talk = npcDefaultTalks.GetValueOrDefault(npcid);
        if (talk == null)
        {
            talk = LifeFactory.Instance.getNPCDefaultTalk(npcid);
            npcDefaultTalks.AddOrUpdate(npcid, talk);
        }

        return talk;
    }

    public NPCConversationManager(IChannelClient c, int npc, ScriptMeta scriptName, List<IPlayer> otherParty, bool test) : base(c)
    {
        this.c = c;
        this.npc = npc;
        this.ScriptMeta = scriptName;
        this.otherParty = otherParty;
    }

    public NPCConversationManager(IChannelClient c, int npc, int oid, ScriptMeta scriptName, bool itemScript) : base(c)
    {
        this.npc = npc;
        this.npcOid = oid;
        this.ScriptMeta = scriptName;
        this.itemScript = itemScript;
        this.otherParty = [];
    }

    public NPCConversationManager(IChannelClient c, int npc, ScriptMeta scriptName) : this(c, npc, -1, scriptName, false)
    {

    }

    public int getNpc()
    {
        return npc;
    }

    public int getNpcObjectId()
    {
        return npcOid;
    }


    public virtual void dispose()
    {
        NextLevelContext.Clear();
        c.CurrentServer.NPCScriptManager.dispose(this);
        getClient().sendPacket(PacketCreator.enableActions());
    }

    public void sendDefault()
    {
        sendOk(getDefaultTalk(npc));
    }

    public void sendNext(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 0, text, "00 01", speaker));
    }

    public void sendPrev(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 0, text, "01 00", speaker));
    }

    public void sendNextPrev(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 0, text, "01 01", speaker));
    }

    public void sendOk(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 0, text, "00 00", speaker));
    }

    public void sendYesNo(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 1, text, "", speaker));
    }

    public void sendAcceptDecline(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 0x0C, text, "", speaker));
    }

    public void sendSimple(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalk(npc, 4, text, "", speaker));
    }

    public void sendStyle(string text, int[] styles)
    {
        if (styles.Length > 0)
        {
            getClient().sendPacket(PacketCreator.getNPCTalkStyle(npc, text, styles));
        }
        else
        {
            // thanks Conrad for noticing empty styles crashing players
            sendOk("Sorry, there are no options of cosmetics available for you here at the moment.");
            dispose();
        }
    }

    public void sendGetNumber(string text, int def, int min, int max, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalkNum(npc, text, def, min, max, speaker));
    }

    public void sendGetText(string text, byte speaker = 0)
    {
        getClient().sendPacket(PacketCreator.getNPCTalkText(npc, text, "", speaker));
    }

    /*
     * 0 = ariant colliseum
     * 1 = Dojo
     * 2 = Carnival 1
     * 3 = Carnival 2
     * 4 = Ghost Ship PQ?
     * 5 = Pyramid PQ
     * 6 = Kerning Subway
     */
    public void sendDimensionalMirror(string text)
    {
        getClient().sendPacket(PacketCreator.getDimensionalMirror(text));
    }

    public void setGetText(string text)
    {
        this._getText = text;
    }

    public string? getText()
    {
        return this._getText;
    }

    public override bool forceStartQuest(int id)
    {
        return forceStartQuest(id, npc);
    }

    public override bool forceCompleteQuest(int id)
    {
        return forceCompleteQuest(id, npc);
    }

    public override bool startQuest(short id)
    {
        return startQuest((int)id);
    }

    public override bool completeQuest(short id)
    {
        return completeQuest((int)id);
    }

    public override bool startQuest(int id)
    {
        return startQuest(id, npc);
    }

    public override bool completeQuest(int id)
    {
        return completeQuest(id, npc);
    }

    public virtual int getMeso()
    {
        return getPlayer().getMeso();
    }

    public virtual void gainMeso(int gain)
    {
        getPlayer().gainMeso(gain, inChat: true);
    }

    public virtual void gainExp(int gain)
    {
        getPlayer().gainExp(gain, true, true);
    }

    public override void showEffect(string effect)
    {
        getPlayer().getMap().broadcastMessage(PacketCreator.environmentChange(effect, 3));
    }

    public void setHair(int hair)
    {
        getPlayer().setHair(hair);
        getPlayer().updateSingleStat(Stat.HAIR, hair);
        getPlayer().equipChanged();
    }

    public void setFace(int face)
    {
        getPlayer().setFace(face);
        getPlayer().updateSingleStat(Stat.FACE, face);
        getPlayer().equipChanged();
    }

    public void setSkin(int color)
    {
        getPlayer().setSkinColor(SkinColorUtils.getById(color));
        getPlayer().updateSingleStat(Stat.SKIN, color);
        getPlayer().equipChanged();
    }

    public int itemQuantity(int itemid)
    {
        return getPlayer().getInventory(ItemConstants.getInventoryType(itemid)).countById(itemid);
    }

    public void displayGuildRanks()
    {
        c.CurrentServerContainer.GuildManager.ShowRankedGuilds(c, npc);
    }

    public bool canSpawnPlayerNpc(int mapid)
    {
        var chr = getPlayer();
        return chr.getLevel() >= chr.getMaxClassLevel()
                && !chr.isGM()
                && c.CurrentServerContainer.PlayerNPCService.CanSpawn(c.CurrentServer.getMapFactory().getMap(mapid), chr.Name);
    }

    public IMapObject? getPlayerNPCByScriptid(int scriptId)
    {
        foreach (var pnpcObj in getPlayer().getMap().getMapObjectsInRange(new Point(0, 0), double.PositiveInfinity, Arrays.asList(MapObjectType.PLAYER_NPC)))
        {
            if (pnpcObj.GetSourceId() == scriptId)
            {
                return pnpcObj;
            }
        }

        return null;
    }

    public override Team? getParty()
    {
        return getPlayer().TeamModel;
    }

    public List<IPlayer>? GetTeamMembers()
    {
        return getParty()?.GetChannelMembers(c.CurrentServer);
    }

    public bool CheckTeamMemberCount(int min, int max)
    {
        var p = getParty();
        if (p == null)
            return false;

        var pCount = p.GetMemberCount();
        return pCount >= min && pCount <= max;
    }

    public bool CheckTeamMemberLevel(int min, int max)
    {
        var p = getParty();
        if (p == null)
            return false;

        var pMember = p.GetTeamMembers();
        return pMember.All(x => x.Level >= min && x.Level <= max);
    }

    public bool CheckTeamMemberChannel()
    {
        var p = getParty();
        if (p == null)
            return false;

        var pMember = p.GetTeamMembers();
        return pMember.All(x => x.Channel == c.CurrentServer.getId());
    }

    public bool CheckTeamMemberMap()
    {
        var p = getParty();
        if (p == null)
            return false;

        return getPlayer().getPartyMembersOnSameMap().Count == p.GetMemberCount();
    }

    public bool CheckTeamMemberLevelRange(int range)
    {
        var p = getParty();
        if (p == null)
            return false;

        var minLevel = p.GetTeamMembers().Min(x => x.Level);
        var maxLevel = p.GetTeamMembers().Max(x => x.Level);
        return maxLevel - minLevel <= range;
    }

    public override void resetMap(int mapid)
    {
        getClient().CurrentServer.getMapFactory().getMap(mapid).resetReactors();
    }

    public void gainTameness(int tameness)
    {
        foreach (var pet in getPlayer().getPets())
        {
            if (pet != null)
            {
                pet.gainTamenessFullness(getPlayer(), tameness, 0, 0);
            }
        }
    }

    public string getName()
    {
        return getPlayer().getName();
    }

    public int getGender()
    {
        return getPlayer().getGender();
    }

    public void changeJobById(int a)
    {
        changeJob(JobFactory.GetById(a));
    }

    public void changeJob(Job job)
    {
        getPlayer().changeJob(job);
    }

    public string getJobName(int id)
    {
        return JobFactory.GetById(id).Name;
    }

    public StatEffect? getItemEffect(int itemId)
    {
        return ItemInformationProvider.getInstance().getItemEffect(itemId);
    }

    public void resetStats()
    {
        getPlayer().resetStats();
    }

    public void openShopNPC(int id)
    {
        var shop = c.CurrentServerContainer.ShopManager.getShop(id);

        if (shop == null)
        {
            // check for missing shopids thanks to resinate
            log.Warning("Shop ID: {ShopId} is missing from database.", id);
            shop = c.CurrentServerContainer.ShopManager.getShop(11000) ?? throw new BusinessResException("ShopId: 11000");
        }
        shop.sendShop(c);
    }

    public void maxMastery()
    {
        var provider = c.CurrentServerContainer.ServiceProvider.GetRequiredService<WzStringProvider>();
        foreach (var skillId in provider.GetAllSkillIdList())
        {
            try
            {
                Skill skill = SkillFactory.GetSkillTrust(skillId);
                getPlayer().changeSkillLevel(skill, 0, skill.getMaxLevel(), -1);
            }
            catch (Exception nfe)
            {
                log.Error(nfe.ToString());
                break;
            }
        }
    }

    public void doGachapon()
    {
        var item = c.CurrentServerContainer.GachaponManager.DoGachapon(npc);

        var itemGained = gainItem(item.ItemId, (short)(ItemConstants.isPotion(item.ItemId) ? 100 : 1), true, true); // For normal potions, make it give 100.

        sendNext("You have obtained a #b#t" + item.ItemId + "##k.");

        int[] maps = {
            MapId.HENESYS,
            MapId.ELLINIA,
            MapId.PERION,
            MapId.KERNING_CITY,
            MapId.SLEEPYWOOD,
            MapId.MUSHROOM_SHRINE,
            MapId.SHOWA_SPA_M,
            MapId.SHOWA_SPA_F,
            MapId.NEW_LEAF_CITY,
            MapId.NAUTILUS_HARBOR
        };
        int mapId = maps[(getNpc() != NpcId.GACHAPON_NAUTILUS && getNpc() != NpcId.GACHAPON_NLC)
            ? (getNpc() - NpcId.GACHAPON_HENESYS)
            : getNpc() == NpcId.GACHAPON_NLC ? 8 : 9];
        string map = c.CurrentServer.getMapFactory().getMap(mapId).getMapName();

        LogFactory.GetLogger(LogType.Gachapon).Information(
            "{CharacterName} got a {ItemName} ({ItemId}) from the {MapName} gachapon.",
            getPlayer().getName(), ItemInformationProvider.getInstance().getName(item.ItemId), item.ItemId, map);

        if (item.Level > 0)
        {
            //Uncommon and Rare
            c.CurrentServerContainer.SendBroadcastWorldPacket(PacketCreator.gachaponMessage(itemGained, map, getPlayer()));
        }
    }

    public void upgradeAlliance()
    {
        c.CurrentServerContainer.GuildManager.HandleIncreaseAllianceCapacity(c.OnlinedCharacter);
    }

    public void disbandAlliance(IChannelClient c, int allianceId)
    {
        c.CurrentServerContainer.GuildManager.DisbandAlliance(c.OnlinedCharacter, allianceId);
    }

    public bool canBeUsedAllianceName(string name)
    {
        return c.CurrentServerContainer.GuildManager.CheckAllianceName(name);
    }

    public Alliance? createAlliance(string name)
    {
        return c.CurrentServerContainer.GuildManager.CreateAlliance(getPlayer(), name);
    }

    public int getAllianceCapacity()
    {
        return getPlayer().AllianceModel!.getCapacity();
    }

    public RemoteHiredMerchantData LoadFredrickRegistry()
    {
        return c.CurrentServerContainer.PlayerShopService.LoadPlayerHiredMerchant(getPlayer());
    }

    public void ShowFredrick(RemoteHiredMerchantData store)
    {
        c.sendPacket(PacketCreator.getFredrick(store));
    }

    public int partyMembersInMap()
    {
        return getPlayer().getMap().getCharacters().Count(x => x.getParty() == getPlayer().getParty());
    }

    public server.events.gm.Event? getEvent()
    {
        return c.CurrentServer.getEvent();
    }

    public void divideTeams()
    {
        if (getEvent() != null)
        {
            getPlayer().setTeam(getEvent()!.getLimit() % 2); //muhaha :D
        }
    }

    public IPlayer? getMapleCharacter(string player)
    {
        return c.CurrentServer.getPlayerStorage().getCharacterByName(player);
    }

    public void logLeaf(string prize)
    {
        MapleLeafLogger.log(getPlayer(), true, prize);
    }

    public bool createPyramid(string mode, bool party)
    {
        //lol
        PyramidMode mod = Enum.Parse<PyramidMode>(mode);

        var partyz = getPlayer().getParty();
        var mapManager = c.CurrentServer.getMapFactory();

        IMap? map = null;
        int mapid = MapId.NETTS_PYRAMID_SOLO_BASE;
        if (party)
        {
            mapid += 10000;
        }
        mapid += ((int)mod * 1000);

        for (byte b = 0; b < 5; b++)
        {
            map = mapManager.getMap(mapid + b);
            //They cannot warp to the next map before the timer ends ( in map = mapManager.getMap(mapid + b);
            if (map.getCharacters().Count > 0)
            {
                continue;
            }
            else
            {
                break;
            }
        }

        if (map == null)
        {
            return false;
        }

        if (!party)
        {
            partyz = new Team(c.CurrentServer.LifeScope.ServiceProvider.GetRequiredService<WorldChannelServer>(), -1, getPlayer().Id);
        }
        Pyramid py = new Pyramid(c.CurrentServer, partyz, mod, map.getId());
        getPlayer().setPartyQuest(py);
        py.warp(mapid);
        dispose();
        return true;
    }

    public bool itemExists(int itemid)
    {
        return ItemInformationProvider.getInstance().getName(itemid) != null;
    }

    public int getCosmeticItem(int itemid)
    {
        if (itemExists(itemid))
        {
            return itemid;
        }

        int baseid;
        if (itemid < 30000)
        {
            baseid = (itemid / 1000) * 1000 + (itemid % 100);
        }
        else
        {
            baseid = (itemid / 10) * 10;
        }

        return itemid != baseid && itemExists(baseid) ? baseid : -1;
    }

    private int getEquippedCosmeticid(int itemid)
    {
        if (itemid < 30000)
        {
            return getPlayer().getFace();
        }
        else
        {
            return getPlayer().getHair();
        }
    }

    public bool isCosmeticEquipped(int itemid)
    {
        return getEquippedCosmeticid(itemid) == itemid;
    }

    public bool isUsingOldPqNpcStyle()
    {
        return YamlConfig.config.server.USE_OLD_GMS_STYLED_PQ_NPCS && this.getPlayer().getParty() != null;
    }

    public int[] getAvailableMasteryBooks()
    {
        return ItemInformationProvider.getInstance().usableMasteryBooks(this.getPlayer()).ToArray();
    }

    public int[] getAvailableSkillBooks()
    {
        List<int> ret = ItemInformationProvider.getInstance().usableSkillBooks(this.getPlayer());
        ret.AddRange(c.CurrentServerContainer.SkillbookInformationProvider.getTeachableSkills(this.getPlayer()));

        return ret.ToArray();
    }

    public object[] getNamesWhoDropsItem(int itemId)
    {
        return MonsterInformationProvider.getInstance().FindDropperNames(itemId).ToArray();
    }

    public string getSkillBookInfo(int itemid)
    {
        var sbe = c.CurrentServerContainer.SkillbookInformationProvider.getSkillbookAvailability(itemid);
        switch (sbe)
        {
            case SkillBookEntry.UNAVAILABLE:
                return "";

            case SkillBookEntry.REACTOR:
                return "    Obtainable through #rexploring#k (loot boxes).";

            case SkillBookEntry.SCRIPT:
                return "    Obtainable through #rexploring#k (field interaction).";

            case SkillBookEntry.QUEST_BOOK:
                return "    Obtainable through #rquestline#k (collecting book).";

            case SkillBookEntry.QUEST_REWARD:
                return "    Obtainable through #rquestline#k (quest reward).";

            default:
                return "    Obtainable through #rquestline#k.";
        }
    }

    // (CPQ + WED wishlist) by -- Drago (Dragohe4rt)
    public int cpqCalcAvgLvl(int map)
    {
        int num = 0;
        int avg = 0;
        foreach (var mmo in c.CurrentServer.getMapFactory().getMap(map).getAllPlayers())
        {
            avg += mmo.getLevel();
            num++;
        }
        avg /= num;
        return avg;
    }

    public bool sendCPQMapLists()
    {
        string msg = LanguageConstants.getMessage(getPlayer(), LanguageConstants.CPQPickRoom);
        int msgLen = msg.Length;
        for (int i = 0; i < 6; i++)
        {
            if (fieldTaken(i))
            {
                if (fieldLobbied(i))
                {
                    msg += "#b#L" + i + "#Carnival Field " + (i + 1) + " (Level: "  // "Carnival field" GMS-like improvement thanks to Jayd (jaydenseah)
                            + cpqCalcAvgLvl(980000100 + i * 100) + " / "
                            + getPlayerCount(980000100 + i * 100) + "x"
                            + getPlayerCount(980000100 + i * 100) + ")  #l\r\n";
                }
            }
            else
            {
                if (i >= 0 && i <= 3)
                {
                    msg += "#b#L" + i + "#Carnival Field " + (i + 1) + " (2x2) #l\r\n";
                }
                else
                {
                    msg += "#b#L" + i + "#Carnival Field " + (i + 1) + " (3x3) #l\r\n";
                }
            }
        }

        if (msg.Length > msgLen)
        {
            sendSimple(msg);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool fieldTaken(int field)
    {
        if (!c.CurrentServer.canInitMonsterCarnival(true, field))
        {
            return true;
        }
        if (c.CurrentServer.getMapFactory().getMap(980000100 + field * 100).getAllPlayer().Count > 0)
        {
            return true;
        }
        if (c.CurrentServer.getMapFactory().getMap(980000101 + field * 100).getAllPlayer().Count > 0)
        {
            return true;
        }
        return c.CurrentServer.getMapFactory().getMap(980000102 + field * 100).getAllPlayer().Count > 0;
    }

    public bool fieldLobbied(int field)
    {
        return c.CurrentServer.getMapFactory().getMap(980000100 + field * 100).getAllPlayer().Count > 0;
    }

    public void cpqLobby(int field)
    {
        try
        {
            IMap map, mapExit;
            var cs = c.CurrentServer;

            map = cs.getMapFactory().getMap(980000100 + 100 * field);
            mapExit = cs.getMapFactory().getMap(980000000);
            foreach (var mc in getPlayer().getParty()!.GetChannelMembers(c.CurrentServer))
            {
                if (mc != null)
                {
                    mc.setChallenged(false);
                    mc.changeMap(map, map.getPortal(0));
                    mc.sendPacket(PacketCreator.serverNotice(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntryLobby)));
                    c.CurrentServerContainer.TimerManager.schedule(() => mapClock(3 * 60), 1500);

                    mc.setCpqTimer(c.CurrentServerContainer.TimerManager.schedule(() => mc.changeMap(mapExit, mapExit.getPortal(0)), TimeSpan.FromMinutes(3)));
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
        }
    }

    public IPlayer? getChrById(int id)
    {
        return c.CurrentServer.getPlayerStorage().getCharacterById(id);
    }

    public void cancelCPQLobby()
    {
        foreach (var mc in getPlayer().getParty()!.GetChannelMembers(c.CurrentServer))
        {
            mc.clearCpqTimer();
        }
    }

    private void warpoutCPQLobby(IMap lobbyMap)
    {
        var outs = lobbyMap.getChannelServer().getMapFactory().getMap((lobbyMap.getId() < 980030000) ? 980000000 : 980030000);
        foreach (var mc in lobbyMap.getAllPlayers())
        {
            mc.resetCP();
            mc.setTeam(-1);
            mc.setMonsterCarnival(null);
            mc.changeMap(outs, outs.getPortal(0));
        }
    }

    private int isCPQParty(IMap lobby, Team party)
    {
        int cpqMinLvl, cpqMaxLvl;

        if (lobby.isCPQLobby())
        {
            cpqMinLvl = 30;
            cpqMaxLvl = 50;
        }
        else
        {
            cpqMinLvl = 51;
            cpqMaxLvl = 70;
        }

        var partyMembers = party.GetChannelMembers(c.CurrentServer);
        foreach (var pchr in partyMembers)
        {
            if (pchr.getLevel() >= cpqMinLvl && pchr.getLevel() <= cpqMaxLvl)
            {
                if (lobby.getCharacterById(pchr.getId()) == null)
                {
                    return 1;  // party member detected out of area
                }
            }
            else
            {
                return 2;  // party member doesn't fit requirements
            }
        }

        return 0;
    }

    private int canStartCPQ(IMap lobby, Team party, Team challenger)
    {
        int ret = isCPQParty(lobby, party);
        if (ret != 0)
        {
            return ret;
        }

        ret = isCPQParty(lobby, challenger);
        if (ret != 0)
        {
            return -ret;
        }

        return 0;
    }

    public void startCPQ(IPlayer? challenger, int field)
    {
        try
        {
            cancelCPQLobby();

            var lobbyMap = getPlayer().getMap();
            if (challenger != null)
            {
                if (challenger.getParty() == null)
                {
                    throw new Exception("No opponent found!");
                }

                foreach (var mc in challenger.getParty()!.GetChannelMembers(c.CurrentServer))
                {
                    mc.changeMap(lobbyMap, lobbyMap.getPortal(0));
                    c.CurrentServerContainer.TimerManager.schedule(() => mapClock(10), 1500);
                }
                foreach (var mc in getPlayer().getParty()!.GetChannelMembers(c.CurrentServer))
                {
                    c.CurrentServerContainer.TimerManager.schedule(() => mapClock(10), 1500);
                }
            }
            int mapid = getPlayer().getMapId() + 1;

            c.CurrentServerContainer.TimerManager.schedule(() =>
            {
                Team lobbyParty = getPlayer().getParty()!, challengerParty = challenger.getParty()!;
                try
                {
                    foreach (var mc in lobbyParty.GetChannelMembers(c.CurrentServer))
                    {
                        mc.setMonsterCarnival(null);
                    }
                    foreach (var mc in challengerParty.GetChannelMembers(c.CurrentServer))
                    {
                        mc.setMonsterCarnival(null);
                    }
                }
                catch (NullReferenceException)
                {
                    warpoutCPQLobby(lobbyMap);
                    return;
                }

                int status = canStartCPQ(lobbyMap, lobbyParty, challengerParty);
                if (status == 0)
                {
                    new MonsterCarnival(c.CurrentServer, lobbyParty, challengerParty, mapid, true, (field / 100) % 10);
                }
                else
                {
                    warpoutCPQLobby(lobbyMap);
                }
            }, 11000);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public void startCPQ2(IPlayer? challenger, int field)
    {
        try
        {
            cancelCPQLobby();

            var lobbyMap = getPlayer().getMap();
            if (challenger != null)
            {
                if (challenger.getParty() == null)
                {
                    throw new Exception("No opponent found!");
                }

                foreach (var mc in challenger.getParty()!.GetChannelMembers(c.CurrentServer))
                {
                    if (mc != null)
                    {
                        mc.changeMap(lobbyMap, lobbyMap.getPortal(0));
                        mapClock(10);
                    }
                }
            }
            int mapid = getPlayer().getMapId() + 100;
            c.CurrentServerContainer.TimerManager.schedule(() =>
            {
                var lobbyParty = getPlayer().getParty()!;
                var challengerParty = challenger.getParty()!;

                try
                {
                    foreach (var mc in lobbyParty.GetChannelMembers(c.CurrentServer))
                    {
                        if (mc != null)
                        {
                            mc.setMonsterCarnival(null);
                        }
                    }
                    foreach (var mc in challengerParty.GetChannelMembers(c.CurrentServer))
                    {
                        if (mc != null)
                        {
                            mc.setMonsterCarnival(null);
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    warpoutCPQLobby(lobbyMap);
                    return;
                }


                int status = canStartCPQ(lobbyMap, lobbyParty, challengerParty);
                if (status == 0)
                {
                    new MonsterCarnival(c.CurrentServer, lobbyParty, challengerParty, mapid, false, (field / 1000) % 10);
                }
                else
                {
                    warpoutCPQLobby(lobbyMap);
                }
            }, 10000);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public bool sendCPQMapLists2()
    {
        string msg = LanguageConstants.getMessage(getPlayer(), LanguageConstants.CPQPickRoom);
        int msgLen = msg.Length;
        for (int i = 0; i < 3; i++)
        {
            if (fieldTaken2(i))
            {
                if (fieldLobbied2(i))
                {
                    msg += "#b#L" + i + "#Carnival Field " + (i + 1) + " (Level: "  // "Carnival field" GMS-like improvement thanks to Jayd
                            + cpqCalcAvgLvl(980031000 + i * 1000) + " / "
                            + getPlayerCount(980031000 + i * 1000) + "x"
                            + getPlayerCount(980031000 + i * 1000) + ")  #l\r\n";
                }
            }
            else
            {
                if (i == 0 || i == 1)
                {
                    msg += "#b#L" + i + "#Carnival Field " + (i + 1) + " (2x2) #l\r\n";
                }
                else
                {
                    msg += "#b#L" + i + "#Carnival Field " + (i + 1) + " (3x3) #l\r\n";
                }
            }
        }

        if (msg.Length > msgLen)
        {
            sendSimple(msg);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool fieldTaken2(int field)
    {
        if (!c.CurrentServer.canInitMonsterCarnival(false, field))
        {
            return true;
        }
        if (c.CurrentServer.getMapFactory().getMap(980031000 + field * 1000).getAllPlayer().Count > 0)
        {
            return true;
        }
        if (c.CurrentServer.getMapFactory().getMap(980031100 + field * 1000).getAllPlayer().Count > 0)
        {
            return true;
        }
        return c.CurrentServer.getMapFactory().getMap(980031200 + field * 1000).getAllPlayer().Count > 0;
    }

    public bool fieldLobbied2(int field)
    {
        return c.CurrentServer.getMapFactory().getMap(980031000 + field * 1000).getAllPlayer().Count > 0;
    }

    public void cpqLobby2(int field)
    {
        try
        {
            IMap map, mapExit;
            var cs = c.CurrentServer;

            mapExit = cs.getMapFactory().getMap(980030000);
            map = cs.getMapFactory().getMap(980031000 + 1000 * field);
            foreach (var mc in c.OnlinedCharacter.getParty()!.GetChannelMembers(c.CurrentServer))
            {
                if (mc != null)
                {
                    mc.setChallenged(false);
                    mc.changeMap(map, map.getPortal(0));
                    mc.sendPacket(PacketCreator.serverNotice(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntryLobby)));
                    c.CurrentServerContainer.TimerManager.schedule(() => mapClock(3 * 60), 1500);

                    mc.setCpqTimer(c.CurrentServerContainer.TimerManager.schedule(() => mc.changeMap(mapExit, mapExit.getPortal(0)), TimeSpan.FromMinutes(3)));
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
        }
    }

    public void mapClock(int time)
    {
        getPlayer().getMap().broadcastMessage(PacketCreator.getClock(time));
    }

    private bool sendCPQChallenge(string cpqType, int leaderid)
    {
        HashSet<int> cpqLeaders = new();
        cpqLeaders.Add(leaderid);
        cpqLeaders.Add(getPlayer().getId());

        return c.CurrentServer.MatchChecker.createMatchConfirmation(MatchCheckerType.CPQ_CHALLENGE, 0, getPlayer().getId(), cpqLeaders, cpqType);
    }

    public void answerCPQChallenge(bool accept)
    {
        c.CurrentServer.MatchChecker.answerMatchConfirmation(getPlayer().getId(), accept);
    }

    public void challengeParty2(int field)
    {
        IPlayer? leader = null;
        var map = c.CurrentServer.getMapFactory().getMap(980031000 + 1000 * field);
        foreach (var mmo in map.getAllPlayer())
        {
            var mc = (IPlayer)mmo;
            if (mc.getParty() == null)
            {
                sendOk(LanguageConstants.getMessage(mc, LanguageConstants.CPQFindError));
                return;
            }
            if (mc.getParty()!.getLeaderId() == mc.getId())
            {
                leader = mc;
                break;
            }
        }
        if (leader != null)
        {
            if (!leader.isChallenged())
            {
                if (!sendCPQChallenge("cpq2", leader.getId()))
                {
                    sendOk(LanguageConstants.getMessage(leader, LanguageConstants.CPQChallengeRoomAnswer));
                }
            }
            else
            {
                sendOk(LanguageConstants.getMessage(leader, LanguageConstants.CPQChallengeRoomAnswer));
            }
        }
        else
        {
            sendOk(LanguageConstants.getMessage(getPlayer(), LanguageConstants.CPQLeaderNotFound));
        }
    }

    public void challengeParty(int field)
    {
        IPlayer? leader = null;
        var map = c.CurrentServer.getMapFactory().getMap(980000100 + 100 * field);
        if (map.getAllPlayer().Count != getPlayer().getParty()!.GetChannelMembers(c.CurrentServer).Count)
        {
            sendOk("An unexpected error regarding the other party has occurred.");
            return;
        }
        foreach (var mmo in map.getAllPlayer())
        {
            var mc = (IPlayer)mmo;
            if (mc.getParty() == null)
            {
                sendOk(LanguageConstants.getMessage(mc, LanguageConstants.CPQFindError));
                return;
            }
            if (mc.getParty()!.getLeaderId() == mc.getId())
            {
                leader = mc;
                break;
            }
        }
        if (leader != null)
        {
            if (!leader.isChallenged())
            {
                if (!sendCPQChallenge("cpq1", leader.getId()))
                {
                    sendOk(LanguageConstants.getMessage(leader, LanguageConstants.CPQChallengeRoomAnswer));
                }
            }
            else
            {
                sendOk(LanguageConstants.getMessage(leader, LanguageConstants.CPQChallengeRoomAnswer));
            }
        }
        else
        {
            sendOk(LanguageConstants.getMessage(getPlayer(), LanguageConstants.CPQLeaderNotFound));
        }
    }

    object setupLock = new object();
    private bool setupAriantBattle(Expedition exped, int mapid)
    {
        lock (setupLock)
        {
            var arenaMap = this.getMap().getChannelServer().getMapFactory().getMap(mapid + 1);
            if (arenaMap.getAllPlayers().Count > 0)
            {
                return false;
            }

            new AriantColiseum(arenaMap, exped);
            return true;
        }
    }

    public string startAriantBattle(ExpeditionType expedType, int mapid)
    {
        if (!GameConstants.isAriantColiseumLobby(mapid))
        {
            return "You cannot start an Ariant tournament from outside the Battle Arena Entrance.";
        }

        var exped = this.getMap().getChannelServer().getExpedition(expedType);
        if (exped == null)
        {
            return "Please register on an expedition before attempting to start an Ariant tournament.";
        }

        var players = exped.getActiveMembers();

        int playersSize = players.Count;
        if (!(playersSize >= exped.getMinSize() && playersSize <= exped.getMaxSize()))
        {
            return "Make sure there are between #r" + exped.getMinSize() + " ~ " + exped.getMaxSize() + " players#k in this room to start the battle.";
        }

        var leaderMap = this.getMap();
        foreach (var mc in players)
        {
            if (mc.getMap() != leaderMap)
            {
                return "All competing players should be on this area to start the battle.";
            }

            if (mc.getParty() != null)
            {
                return "All competing players must not be on a party to start the battle.";
            }

            int level = mc.getLevel();
            if (!(level >= expedType.getMinLevel() && level <= expedType.getMaxLevel()))
            {
                return "There are competing players outside of the acceptable level range in this room. All players must be on #blevel between 20~30#k to start the battle.";
            }
        }

        if (setupAriantBattle(exped, mapid))
        {
            return "";
        }
        else
        {
            return "Other players are already competing on the Ariant tournament in this room. Please wait a while until the arena becomes available again.";
        }
    }



    #region NextLevelTalk
    /// <summary>
    /// 只有下一步的对话
    /// 对应sendNext
    /// </summary>
    /// <param name="nextLevel">下一步方法 function level{nextLevel}</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker">说话者，0,1,8,9 = NPC；2,3 = 玩家；4,5,6,7 = 客户端报38错误；其它数字未测试。</param>
    public void sendNextLevel(string? nextLevel, string text, byte speaker = 0)
    {
        sendNext(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_NEXT, nextLevel);
    }
    /// <summary>
    /// 只有下一步的对话
    /// 对应sendNext
    /// </summary>
    /// <param name="text">对话内容</param>
    /// <param name="speaker">说话者，0,1,8,9 = NPC；2,3 = 玩家；4,5,6,7 = 客户端报38错误；其它数字未测试。</param>
    public void sendNextLevel(string text, byte speaker = 0)
    {
        sendNextLevel(null, text, speaker);
    }

    /// <summary>
    /// 只有上一步的对话
    /// 对应sendPrev
    /// </summary>
    /// <param name="lastLevel">上一步方法 function level{lastLevel}</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker">说话者，0,1,8,9 = NPC；2,3 = 玩家；4,5,6,7 = 客户端报38错误；其它数字未测试。</param>
    public void sendLastLevel(string lastLevel, string text, byte speaker = 0)
    {
        sendPrev(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_LAST, lastLevel);
    }

    /// <summary>
    /// 有上一步和下一步的对话
    /// 对应sendNextPrev
    /// </summary>
    /// <param name="lastLevel">上一步方法</param>
    /// <param name="nextLevel">下一步方法</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker">说话者，0,1,8,9 = NPC；2,3 = 玩家；4,5,6,7 = 客户端报38错误；其它数字未测试。</param>
    public void sendLastNextLevel(string lastLevel, string nextLevel, string text, byte speaker = 0)
    {
        sendNextPrev(text, speaker);
        NextLevelContext.TwoOption(NextLevelType.SEND_LAST_NEXT, lastLevel, nextLevel);
    }

    /// <summary>
    /// 只有ok按钮的对话
    /// 对应sendOk，OK后调用level{nextLevel}
    /// </summary>
    /// <param name="nextLevel">点击ok的下一步方法</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker">说话者，0,1,8,9 = NPC；2,3 = 玩家；4,5,6,7 = 客户端报38错误；其它数字未测试。</param>
    public void sendOkLevel(string nextLevel, string text, byte speaker = 0)
    {
        sendOk(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_OK, nextLevel);
    }

    /// <summary>
    /// 只有ok按钮的对话
    /// 对应sendOk，OK后直接结束，dispose
    /// </summary>
    /// <param name="text">对话内容</param>
    /// <param name="speaker">说话者，0,1,8,9 = NPC；2,3 = 玩家；4,5,6,7 = 客户端报38错误；其它数字未测试。</param>
    public void sendOkLevel(string text, byte speaker = 0)
    {
        sendOk(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_OK, null);
    }

    /**
     * 多个选项的对话，选择后自动路由到level + selection对应的方法
     * 对应sendSimple
     *
     * @param text 对话内容
     */
    public void sendSelectLevel(string text, byte speaker = 0)
    {
        sendSelectLevel("", text, speaker);
    }

    /**
     * 多个选项的对话，选择后自动路由到level + prefix + selection对应的方法
     * 对应sendSimple
     *
     * @param prefix 方法前缀，如果脚本有多次要选择的地方，可以通过不同的前缀区分
     * @param text   对话内容
     */
    public void sendSelectLevel(string prefix, string text, byte speaker = 0)
    {
        sendSimple(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_SELECT, prefix);
    }

    /**
     * 多个选项的对话，选择后路由到指定方法，将玩家的选择传入
     * 对应sendSimple
     *
     * @param nextLevel 方法前缀，如果脚本有多次要选择的地方，可以通过不同的前缀区分
     * @param text   对话内容
     */
    public void sendNextSelectLevel(string nextLevel, string text, byte speaker = 0)
    {
        sendSimple(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_NEXT_SELECT, nextLevel);
    }

    /// <summary>
    /// 获取玩家输入数字的对话
    /// 对应sendGetNumber
    /// </summary>
    /// <param name="nextLevel">下一步方法</param>
    /// <param name="text">对话内容</param>
    /// <param name="def">默认值</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    /// <param name="speaker"></param>
    public void getInputNumberLevel(string nextLevel, string text, int def, int min, int max, byte speaker = 0)
    {
        sendGetNumber(text, def, min, max, speaker);
        NextLevelContext.OneOption(NextLevelType.GET_INPUT_NUMBER, nextLevel);
    }


    /// <summary>
    /// 获取玩家输入字符串的对话
    /// 对应sendGetText
    /// </summary>
    /// <param name="nextLevel">下一步</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker"></param>
    public void getInputTextLevel(string nextLevel, string text, byte speaker = 0)
    {
        sendGetText(text, speaker);
        NextLevelContext.OneOption(NextLevelType.GET_INPUT_TEXT, nextLevel);
    }

    /// <summary>
    /// 有接受和拒绝的对话
    /// 对应sendAcceptDecline
    /// </summary>
    /// <param name="decLineLevel">拒绝</param>
    /// <param name="acceptLevel">接受</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker"></param>
    public void sendAcceptDeclineLevel(string decLineLevel, string acceptLevel, string text, byte speaker = 0)
    {
        sendAcceptDecline(text, speaker);
        NextLevelContext.TwoOption(NextLevelType.SEND_ACCEPT_DECLINE, decLineLevel, acceptLevel);
    }

    /// <summary>
    /// 有是和否的对话
    /// 对应sendYesNo
    /// </summary>
    /// <param name="noLevel">否方法</param>
    /// <param name="yesLevel">是方法</param>
    /// <param name="text">对话内容</param>
    /// <param name="speaker"></param>
    public void sendYesNoLevel(string noLevel, string yesLevel, string text, byte speaker = 0)
    {
        sendYesNo(text, speaker);
        NextLevelContext.TwoOption(NextLevelType.SEND_YES_NO, noLevel, yesLevel);
    }

    public void SetContextData(object? data)
    {
        NextLevelContext.SetContextData(data);
    }

    public object? GetContextData()
    {
        return NextLevelContext.GetContextData();
    }
    #endregion

    public int[] getCardTierSize()
    {
        return ItemInformationProvider.getInstance().getCardTierSize();
    }
}