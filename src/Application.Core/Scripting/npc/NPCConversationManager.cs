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


using AllianceProto;
using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Managers;
using Application.Core.Models;
using Application.Core.scripting.Infrastructure;
using GuildProto;
using server;
using server.partyquest;
using System.Threading.Channels;
using System.Threading.Tasks;
using tools;
using static server.partyquest.Pyramid;


namespace scripting.npc;

/**
 * @author Matze
 */
public class NPCConversationManager : AbstractPlayerInteraction, IAsyncDisposable
{
    private ILogger log = LogFactory.GetLogger(LogType.Conversation);

    protected int npc;
    private int npcOid;
    public ScriptMeta ScriptMeta { get; }
    private string? _getText;

    public NextLevelContext NextLevelContext { get; set; } = new NextLevelContext();


    public NPCConversationManager(IChannelClient c, int npc, int oid, ScriptMeta scriptName) : base(c)
    {
        this.npc = npc;
        this.npcOid = oid;
        this.ScriptMeta = scriptName;
    }

    public NPCConversationManager(IChannelClient c, int npc, ScriptMeta scriptName) : this(c, npc, -1, scriptName)
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

    public virtual async ValueTask DisposeAsync()
    {
        _talkChannel.Writer.Complete();
        NextLevelContext.Clear();
        await c.CurrentServer.NPCScriptManager.dispose(this);
        await c.SendPacket(PacketCreator.enableActions());
        c.OnlinedCharacter.removeClickedNPC();
    }

    public async Task sendDefault(int checkQuestId = 0)
    {
        await sendOk(GetDefaultTalk(checkQuestId));
    }

    public string GetDefaultTalk(int checkQuestId = 0)
    {
        if (checkQuestId > 0)
            return isQuestCompleted(checkQuestId) ? GetDefault1() : GetDefault0();

        return GetDefaultRandom();
    }

    public string GetDefault0() => c.CurrentCulture.GetNpcDefaultTalk(npc, 0);
    public string GetDefault1() => c.CurrentCulture.GetNpcDefaultTalk(npc, 1);
    public string GetDefaultRandom() => c.CurrentCulture.GetNpcDefaultTalk(npc, -1);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="param">使用 params string[] 时，可能报错</param>
    /// <returns></returns>
    public string GetTalkMessage(string text, params object[] param) => c.CurrentCulture.GetScriptTalkByKey(text, param);
    public string GetClientMessage(string text, params object[] param) => c.CurrentCulture.GetMessageByKey(text, param);

    public async Task sendNext(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 0, text, "00 01", speaker, speakerNpc));
    }

    public async Task sendPrev(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 0, text, "01 00", speaker, speakerNpc));
    }

    public async Task sendNextPrev(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 0, text, "01 01", speaker, speakerNpc));
    }

    public async Task sendOk(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 0, text, "00 00", speaker, speakerNpc));
    }

    public async Task sendYesNo(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 1, text, "", speaker, speakerNpc));
    }

    public async Task sendAcceptDecline(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 12, text, "", speaker, speakerNpc));
    }

    public async Task sendSimple(string text, byte speaker = 0, int speakerNpc = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalk(npc, 4, text, "", speaker, speakerNpc));
    }

    public async Task sendStyle(string text, int[] styles)
    {
        if (styles.Length > 0)
        {
            await getClient().SendPacket(PacketCreator.getNPCTalkStyle(npc, text, styles));
        }
        else
        {
            // thanks Conrad for noticing empty styles crashing players
            await sendOk("Sorry, there are no options of cosmetics available for you here at the moment.");
            await DisposeAsync();
        }
    }

    public async Task sendGetNumber(string text, int def, int min, int max, byte speaker = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalkNum(npc, text, def, min, max, speaker));
    }

    public async Task sendGetText(string text, byte speaker = 0)
    {
        await getClient().SendPacket(PacketCreator.getNPCTalkText(npc, text, "", speaker));
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
    public async Task sendDimensionalMirror(string text)
    {
        await getClient().SendPacket(PacketCreator.getDimensionalMirror(text));
    }

    public void setGetText(string text)
    {
        this._getText = text;
    }

    public string? getText()
    {
        return this._getText;
    }

    public async Task<bool> forceStartQuest(int id)
    {
        return await base.startQuest((short)id, npc);
    }

    public async Task<bool> forceCompleteQuest(int id)
    {
        return await base.completeQuest((short)id, npc);
    }

    public Task<bool> startQuest(int id)
    {
        return forceStartQuest(id);
    }

    public Task<bool> completeQuest(int id)
    {
        return forceCompleteQuest(id);
    }

    public virtual int getMeso()
    {
        return getPlayer().getMeso();
    }

    public virtual async Task gainMeso(int gain)
    {
        await getPlayer().GainMeso(gain, GainItemShow.ShowInChat);
    }

    public virtual async Task gainExp(int gain)
    {
        await getPlayer().gainExp(gain, true, true);
    }

    public override async Task showEffect(string effect)
    {
        await getPlayer().getMap().broadcastMessage(PacketCreator.environmentChange(effect, 3));
    }

    public async Task setHair(int hair)
    {
        getPlayer().setHair(hair);
        await getPlayer().updateSingleStat(Stat.HAIR, hair);
        await getPlayer().equipChanged();
    }

    public async Task setFace(int face)
    {
        getPlayer().setFace(face);
        await getPlayer().updateSingleStat(Stat.FACE, face);
        await getPlayer().equipChanged();
    }

    public async Task setSkin(int color)
    {
        getPlayer().setSkinColor(SkinColorUtils.getById(color));
        await getPlayer().updateSingleStat(Stat.SKIN, color);
        await getPlayer().equipChanged();
    }

    public int itemQuantity(int itemid)
    {
        return getPlayer().getInventory(ItemConstants.getInventoryType(itemid)).countById(itemid);
    }

    public async Task displayGuildRanks()
    {
        await c.CurrentServer.NodeService.GuildManager.ShowRankedGuilds(c, npc);
    }

    public async Task<bool> canSpawnPlayerNpc(int mapid)
    {
        var chr = getPlayer();
        return chr.getLevel() >= chr.getMaxClassLevel()
                && !chr.isGM()
                && c.CurrentServer.NodeService.PlayerNPCService.CanSpawnHonor(await c.CurrentServer.getMapFactory().getMap(mapid), chr.Name);
    }

    public IMapObject? getPlayerNPCByScriptid(int scriptId)
    {
        foreach (var pnpcObj in getPlayer().getMap().GetMapObjects(x => x.getType() == MapObjectType.PLAYER_NPC))
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
        return getPlayer().getParty();
    }

    public List<Player>? GetTeamMembers()
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

    public override async Task resetMap(int mapid)
    {
        await (await getClient().CurrentServer.getMapFactory().getMap(mapid)).resetReactors();
    }

    public async Task gainTameness(int tameness)
    {
        foreach (var pet in getPlayer().getPets())
        {
            if (pet != null)
            {
                await pet.gainTamenessFullness(tameness, 0, 0);
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

    public async Task changeJobById(int a)
    {
        await changeJob(JobFactory.GetById(a));
    }

    public async Task changeJob(Job job)
    {
        await getPlayer().changeJob(job);
    }

    public string getJobName(int id)
    {
        return GetJobName(JobFactory.GetById(id));
    }


    public async Task resetStats()
    {
        await getPlayer().resetStats();
    }

    public async Task openShopNPC(int id)
    {
        var shop = c.CurrentServer.NodeService.ShopManager.getShop(id);

        if (shop == null)
        {
            // check for missing shopids thanks to resinate
            log.Warning("Shop ID: {ShopId} is missing from database.", id);
            shop = c.CurrentServer.NodeService.ShopManager.getShop(11000) ?? throw new BusinessResException("ShopId: 11000");
        }
        await shop.sendShop(c);
    }

    public async Task maxMastery()
    {
        var provider = ClientCulture.SystemCulture.StringProvider;
        foreach (var skillData in provider.GetSubProvider(Application.Templates.String.StringCategory.Skill).LoadAll())
        {
            try
            {
                Skill skill = SkillFactory.GetSkillTrust(skillData.TemplateId);
                await getPlayer().changeSkillLevel(skill, 0, skill.getMaxLevel(), -1);
            }
            catch (Exception nfe)
            {
                log.Error(nfe.ToString());
                break;
            }
        }
    }

    public async Task OpenStorage()
    {
        await c.OnlinedCharacter.Storage.OpenStorage(npc);
    }

    public bool CheckGachaponStorage(int willGot)
    {
        return c.OnlinedCharacter.GachaponStorage.CanGainItem(willGot);
    }

    public async Task OpenGachaponStorage()
    {
        await c.OnlinedCharacter.GachaponStorage.OpenStorage(npc);
    }
    static Dictionary<int, int> gachaponNpcMapMapping = new Dictionary<int, int>()
    {
        { NpcId.GACHAPON_HENESYS, MapId.HENESYS },
        { NpcId.GACHAPON_ELLINIA, MapId.ELLINIA },
        { NpcId.GACHAPON_PERION, MapId.PERION },
        { NpcId.GACHAPON_KERNING, MapId.KERNING_CITY },
        { NpcId.GACHAPON_SLEEPYWOOD, MapId.SLEEPYWOOD },
        { NpcId.GRANDPA_MOON_BUNNY, MapId.MUSHROOM_SHRINE },
        { NpcId.GACHAPON_SHOWA_MALE, MapId.SHOWA_SPA_M },
        { NpcId.GACHAPON_SHOWA_FEMALE, MapId.SHOWA_SPA_F },
        { NpcId.GACHAPON_NLC, MapId.NEW_LEAF_CITY },
        { NpcId.GACHAPON_EL_NATH, MapId.EL_NATH },
        { NpcId.GACHAPON_NAUTILUS, MapId.NAUTILUS_HARBOR },
    };
    public string GetGachaponMapName()
    {
        return c.CurrentCulture.GetMapName(gachaponNpcMapMapping.GetValueOrDefault(getNpc()));
    }
    public async Task<GachaponPoolItemDataObject?> doGachapon()
    {
        var reward = c.CurrentServer.NodeService.GachaponManager.DoGachapon(npc);
        var rewardItem = ItemInformationProvider.getInstance().GenerateVirtualItemById(reward.ItemId, reward.Quantity, true);
        if (rewardItem == null)
        {
            LogFactory.GetLogger(LogType.Gachapon).Debug("ItemId={ItemId} not found", reward.ItemId);
            return null;
        }

        if (!await c.OnlinedCharacter.GachaponStorage.PutItem(rewardItem))
            return null;

        string map = ClientCulture.SystemCulture.GetMapName(gachaponNpcMapMapping.GetValueOrDefault(getNpc()));

        LogFactory.GetLogger(LogType.Gachapon).Information(
            "{CharacterName} got a {ItemName} ({ItemId}) from the {MapName} gachapon.",
            getPlayer().getName(), ClientCulture.SystemCulture.GetItemName(reward.ItemId), reward.ItemId, map);

        if (reward.Level > 1)
        {
            //Uncommon and Rare
            await c.CurrentServer.NodeService.SendBroadcastWorldPacket(PacketCreator.gachaponMessage(rewardItem, map, getPlayer()));
        }
        return reward;
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

    public void logLeaf(string prize)
    {
        MapleLeafLogger.log(getPlayer(), true, prize);
    }

    public async Task<bool> createPyramid(string mode, bool party)
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
            map = await mapManager.getMap(mapid + b);
            //They cannot warp to the next map before the timer ends ( in map = mapManager.getMap(mapid + b);
            if (map.getAllPlayers().Count > 0)
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
            partyz = new Team(-1, getPlayer().Id);
        }
        Pyramid py = new Pyramid(c.CurrentServer, partyz, mod, map.getId());
        getPlayer().setPartyQuest(py);
        await py.warp(mapid);
        await DisposeAsync();
        return true;
    }

    public bool itemExists(int itemid)
    {
        return ItemInformationProvider.getInstance().HasTemplate(itemid);
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
        return YamlConfig.config.server.USE_OLD_GMS_STYLED_PQ_NPCS && this.getPlayer().Party > 0;
    }

    public int[] getAvailableMasteryBooks()
    {
        return ItemInformationProvider.getInstance().usableMasteryBooks(this.getPlayer()).ToArray();
    }

    public int[] getAvailableSkillBooks()
    {
        List<int> ret = ItemInformationProvider.getInstance().usableSkillBooks(this.getPlayer());
        ret.AddRange(getTeachableSkills(this.getPlayer()));

        return ret.ToArray();
    }

    public List<int> getTeachableSkills(Player chr)
    {
        List<int> list = new();

        foreach (int book in c.CurrentServer.NodeService.SkillbookInformationProvider.GetAllSkills().Keys)
        {
            if (book >= 0)
            {
                continue;
            }

            int skillid = -book;
            if (skillid / 10000 == chr.getJob().getId())
            {
                if (chr.getMasterLevel(skillid) == 0)
                {
                    list.Add(-skillid);
                }
            }
        }

        return list;
    }

    public object[] getNamesWhoDropsItem(int itemId)
    {
        return MonsterInformationProvider.getInstance().FindDropperNames(c, itemId).ToArray();
    }

    public string getSkillBookInfo(int itemid)
    {
        var sbe = c.CurrentServer.NodeService.SkillbookInformationProvider.getSkillbookAvailability(itemid);
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


    public void mapClock(int time)
    {
        getPlayer().getMap().broadcastMessage(PacketCreator.getClock(time));
    }


    #region New Talk
    Channel<TalkMoreAction> _talkChannel = System.Threading.Channels.Channel.CreateBounded<TalkMoreAction>(1);

    public async Task Response(sbyte mode, sbyte type, int selection, string? inputText = null)
    {
        await _talkChannel.Writer.WriteAsync(new TalkMoreAction(mode, type, selection, inputText));
    }

    async Task<bool> WaitingForAnswer()
    {
        var action = await _talkChannel.Reader.ReadAsync();
        if (action.Mode == -1)
        {
            throw new ConversationInterruptException();
        }

        return action.Mode > 0;
    }

    async Task<int> WaitingForOption()
    {
        var action = await _talkChannel.Reader.ReadAsync();
        if (action.Mode <= 0)
        {
            throw new ConversationInterruptException();
        }
        return action.selection;
    }

    async Task<int> WaitingForInputNumber()
    {
        var action = await _talkChannel.Reader.ReadAsync();
        if (action.Mode <= 0)
        {
            throw new ConversationInterruptException();
        }
        return action.selection;
    }

    async Task<string?> WaitingForInputText()
    {
        var action = await _talkChannel.Reader.ReadAsync();
        if (action.Mode <= 0)
        {
            throw new ConversationInterruptException();
        }
        return action.inputText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="speaker"></param>
    /// <param name="speakerNpc">在 speaker ==  <see cref="NpcTalkSpeaker.ExtraNpc"/>时有效 </param>
    /// <returns></returns>
    public async Task SayNext(string? text, NpcTalkSpeaker speaker = NpcTalkSpeaker.NpcLeft, int speakerNpc = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        await sendNext(text, (byte)speaker, speakerNpc);
        await WaitingForAnswer();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="speaker"></param>
    /// <param name="finalNext">最后一段显示下一步</param>
    /// <returns></returns>
    public async Task SaySpeech(string[] messages, int current = 0, bool finalNext = true)
    {
        while (current >= 0 && current < messages.Length)
        {
            var text = messages[current];
            if (current == 0)
            {
                await sendNext(text, 0);
                if (await WaitingForAnswer())
                {
                    current++;
                }
            }
            else if (current == messages.Length - 1)
            {
                if (finalNext)
                {
                    await sendNextPrev(text, 0);
                    current += (await WaitingForAnswer()) ? 1 : -1;
                }
                else
                {
                    await sendPrev(text, 0);
                    if (!await WaitingForAnswer())
                    {
                        current--;
                    }
                }
            }
            else
            {
                await sendNextPrev(text, 0);
                current += (await WaitingForAnswer()) ? 1 : -1;
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="speaker"></param>
    /// <param name="finalNext">最后一段显示下一步</param>
    /// <returns></returns>
    public async Task SaySpeech(SpeechText[] messages, int current = 0, bool finalNext = true)
    {
        while (current >= 0 && current < messages.Length)
        {
            var text = messages[current];
            if (current == 0)
            {
                await sendNext(text.Text, (byte)text.Speaker, text.SpeakerNpc);
                if (await WaitingForAnswer())
                {
                    current++;
                }
            }
            else if (current == messages.Length - 1)
            {
                if (finalNext)
                {
                    await sendNextPrev(text.Text, (byte)text.Speaker, text.SpeakerNpc);
                    current += (await WaitingForAnswer()) ? 1 : -1;
                }
                else
                {
                    await sendPrev(text.Text, (byte)text.Speaker, text.SpeakerNpc);
                    if (!await WaitingForAnswer())
                    {
                        current--;
                    }
                }
            }
            else
            {
                await sendNextPrev(text.Text, (byte)text.Speaker, text.SpeakerNpc);
                current += (await WaitingForAnswer()) ? 1 : -1;
            }
        }
    }


    public async Task SayOK(string? text, byte speaker = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        await sendOk(text, speaker);
        await WaitingForAnswer();
    }

    public async Task<bool> AskYesNo(string text, byte speaker = 0)
    {
        await sendYesNo(text, speaker);
        return await WaitingForAnswer();
    }

    public async Task<bool> SayAcceptDecline(string text, byte speaker = 0)
    {
        await sendAcceptDecline(text, speaker);
        return await WaitingForAnswer();
    }

    public async Task<int> AskMenu(string text, byte speaker = 0)
    {
        await sendSimple(text, speaker);
        return await WaitingForOption();
    }

    public async Task<int> AskDimensionalMirror(string text)
    {
        await getClient().SendPacket(PacketCreator.getDimensionalMirror(text));
        return await WaitingForOption();
    }

    public async Task<int> AskMenu(string mainContent, IEnumerable<string> options, byte speaker = 0)
    {
        if (options.Count() == 0)
        {
            await SayOK(mainContent);
            throw new ConversationInterruptException();
        }

        var finalContent = mainContent + "\r\n#b";
        for (int i = 0; i < options.Count(); i++)
        {
            finalContent += $"#L{i}#{options.ElementAt(i)}#l\r\n";
        }
        finalContent += "#k";
        return await AskMenu(finalContent, speaker);
    }

    public async Task<int> AskMenu(string mainContent, Dictionary<int, string> options, byte speaker = 0)
    {
        if (options.Count() == 0)
        {
            await SayOK(mainContent);
            throw new ConversationInterruptException();
        }

        var finalContent = mainContent + "\r\n#b";
        foreach (var item in options)
        {
            finalContent += $"#L{item.Key}#{item.Value}#l\r\n";
        }
        finalContent += "#k";
        return await AskMenu(finalContent, speaker);
    }

    public async Task<int> AskAvatar(string text, int[] styles)
    {
        if (styles.Length > 0)
        {
            await sendStyle(text, styles);
            return await WaitingForOption();
        }
        else
        {
            // thanks Conrad for noticing empty styles crashing players
            await SayOK("Sorry, there are no options of cosmetics available for you here at the moment.");
            return -1;
        }
    }

    public async Task<int> AskNumber(string text, int def, int min, int max, byte speaker = 0)
    {
        await sendGetNumber(text, def, min, max, speaker);
        return await WaitingForInputNumber();
    }

    public async Task<string?> AskText(string text, byte speaker = 0)
    {
        await sendGetText(text, speaker);
        return await WaitingForInputText();
    }
    #endregion


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

    public void SendParamedNextLevel(string level, object nextParam, string text, byte speaker = 0)
    {
        sendNext(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_NEXT, level, nextParam);
    }

    public void SendParamedLastLevel(string level, object lastParam, string text, byte speaker = 0)
    {
        sendPrev(text, speaker);
        NextLevelContext.OneOption(NextLevelType.SEND_LAST, level, lastParam);
    }

    public void SendParamedLastNextLevel(string level, object lastLevelParam, object nextLevelParam, string text, byte speaker = 0)
    {
        sendNextPrev(text, speaker);
        NextLevelContext.TwoOption(NextLevelType.SEND_LAST_NEXT, level, level, lastLevelParam, nextLevelParam);
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

    #region Guild/Alliance Operation
    public GuildDto? GetGuild() => getPlayer().GetGuild();
    public AllianceDto? GetAlliance() => getPlayer().GetAlliance();

    public async Task upgradeAlliance()
    {
        await c.CurrentServer.NodeService.GuildManager.HandleIncreaseAllianceCapacity(c.OnlinedCharacter);
    }

    public async Task disbandAlliance(IChannelClient c, int allianceId)
    {
        await c.CurrentServer.NodeService.GuildManager.DisbandAlliance(c.OnlinedCharacter, allianceId);
    }

    public bool canBeUsedAllianceName(string name)
    {
        return c.CurrentServer.NodeService.GuildManager.CheckAllianceName(name);
    }

    public async Task CreateAllianceAysnc(string name, int cost)
    {
        await c.CurrentServer.NodeService.GuildManager.CreateAlliance(getPlayer(), name, cost);
    }

    public int getAllianceCapacity()
    {
        return getPlayer().GetAlliance()?.Capacity ?? 0;
    }

    public async Task increaseGuildCapacity()
    {
        var guild = GetGuild();
        if (guild == null)
            return;

        int cost = GuildManager.getIncreaseGuildCost(guild.Capacity);

        if (getMeso() < cost)
        {
            await dropMessage(1, "You don't have enough mesos.");
            return;
        }

        await c.CurrentServer.NodeService.GuildManager.IncreaseGuildCapacity(getPlayer(), cost);
    }

    public async Task disbandGuild()
    {
        if (getPlayer().GuildId < 1 || getPlayer().GuildRank != 1)
        {
            return;
        }
        try
        {
            await c.CurrentServer.NodeService.GuildManager.Disband(getPlayer());
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    #endregion
}