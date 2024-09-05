/*
    This file is part of the HeavenMS MapleStory NewServer, commands OdinMS-based
    Copyleft (L) 2016 - 2019 RonanLana

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

/*
   @Author: Arthur L - Refactored command content into modules
*/


using client.command.commands.gm0;
using client.command.commands.gm1;
using client.command.commands.gm2;
using client.command.commands.gm3;
using client.command.commands.gm4;
using client.command.commands.gm5;
using client.command.commands.gm6;
using constants.id;

namespace client.command;






public class CommandsExecutor
{
    private static ILogger log = LogFactory.GetLogger("CommandsExecutor");
    private static CommandsExecutor instance = new CommandsExecutor();
    private static char USER_HEADING = '@';
    private static char GM_HEADING = '!';

    private Dictionary<string, Command> registeredCommands = new();
    private List<KeyValuePair<List<string>, List<string>>> commandsNameDesc = new();
    private KeyValuePair<List<string>, List<string>> levelCommandsCursor;

    public static CommandsExecutor getInstance()
    {
        return instance;
    }

    public static bool isCommand(IClient client, string content)
    {
        char heading = content.ElementAt(0);
        if (client.OnlinedCharacter.isGM())
        {
            return heading == USER_HEADING || heading == GM_HEADING;
        }
        return heading == USER_HEADING;
    }

    private CommandsExecutor()
    {
        registerLv0Commands();
        registerLv1Commands();
        registerLv2Commands();
        registerLv3Commands();
        registerLv4Commands();
        registerLv5Commands();
        registerLv6Commands();
    }

    public List<KeyValuePair<List<string>, List<string>>> getGmCommands()
    {
        return commandsNameDesc;
    }

    public void handle(IClient client, string message)
    {
        if (client.tryacquireClient())
        {
            try
            {
                handleInternal(client, message);
            }
            finally
            {
                client.releaseClient();
            }
        }
        else
        {
            client.OnlinedCharacter.dropMessage(5, "Try again in a while... Latest commands are currently being processed.");
        }
    }

    private void handleInternal(IClient client, string message)
    {
        if (client.OnlinedCharacter.getMapId() == MapId.JAIL)
        {
            client.OnlinedCharacter.yellowMessage("You do not have permission to use commands while in jail.");
            return;
        }
        string splitRegex = " ";
        string[] SplitedMessage = message.Substring(1).Split(splitRegex, 2);
        if (SplitedMessage.Length < 2)
        {
            SplitedMessage = new string[] { SplitedMessage[0], "" };
        }

        client.OnlinedCharacter.setLastCommandMessage(SplitedMessage[1]);    // thanks Tochi & Nulliphite for noticing string messages being marshalled lowercase
        string commandName = SplitedMessage[0].ToLower();
        string[] lowercaseParams = SplitedMessage[1].ToLower().Split(splitRegex);

        var command = registeredCommands.GetValueOrDefault(commandName);
        if (command == null)
        {
            client.OnlinedCharacter.yellowMessage("Command '" + commandName + "' is not available. See @commands for a list of available commands.");
            return;
        }
        if (client.OnlinedCharacter.gmLevel() < command.getRank())
        {
            client.OnlinedCharacter.yellowMessage("You do not have permission to use this command.");
            return;
        }
        string[] paramsValue;
        if (lowercaseParams.Length > 0 && lowercaseParams[0].Count() > 0)
        {
            paramsValue = Arrays.copyOfRange(lowercaseParams, 0, lowercaseParams.Length);
        }
        else
        {
            paramsValue = new string[] { };
        }

        command.execute(client, paramsValue);
        log.Information("Chr {CharacterName} used command {Command}", client.OnlinedCharacter.getName(), command.GetType().Name);
    }

    private void addCommandInfo<T>(string name) where T : Command
    {
        try
        {
            levelCommandsCursor.Value.Add(Activator.CreateInstance<T>().getDescription());
            levelCommandsCursor.Key.Add(name);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private void addCommand<T>(string[] syntaxs, int rank = 0) where T : Command
    {
        foreach (string syntax in syntaxs)
        {
            addCommand<T>(syntax, 0);
        }
    }


    private void addCommand<T>(string syntax, int rank = 0) where T : Command
    {
        if (registeredCommands.ContainsKey(syntax.ToLower()))
        {
            log.Warning("Error on register command with name: {Syntax}. Already exists.", syntax);
            return;
        }

        string commandName = syntax.ToLower();
        addCommandInfo<T>(commandName);

        try
        {
            Command commandInstance = Activator.CreateInstance<T>();         // thanks Halcyon for noticing commands getting reinstanced every call
            commandInstance.setRank(rank);

            registeredCommands.Add(commandName, commandInstance);
        }
        catch (Exception e)
        {
            log.Warning(e, "Failed to create command instance");
        }
    }

    private void registerLv0Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<HelpCommand>(new string[] { "help", "commands" });
        addCommand<DropLimitCommand>("droplimit");
        addCommand<TimeCommand>("time");
        addCommand<StaffCommand>("credits");
        addCommand<UptimeCommand>("uptime");
        addCommand<GachaCommand>("gacha");
        addCommand<DisposeCommand>("dispose");
        addCommand<ChangeLanguageCommand>("changel");
        addCommand<EquipLvCommand>("equiplv");
        addCommand<ShowRatesCommand>("showrates");
        addCommand<RatesCommand>("rates");
        addCommand<OnlineCommand>("online");
        addCommand<GmCommand>("gm");
        addCommand<ReportBugCommand>("reportbug");
        addCommand<ReadPointsCommand>("points");
        addCommand<JoinEventCommand>("joinevent");
        addCommand<LeaveEventCommand>("leaveevent");
        addCommand<RanksCommand>("ranks");
        addCommand<StatStrCommand>("str");
        addCommand<StatDexCommand>("dex");
        addCommand<StatIntCommand>("int");
        addCommand<StatLukCommand>("luk");
        addCommand<EnableAuthCommand>("enableauth");
        addCommand<ToggleExpCommand>("toggleexp");
        addCommand<MapOwnerClaimCommand>("mylawn");
        addCommand<BossHpCommand>("bosshp");
        addCommand<MobHpCommand>("mobhp");

        commandsNameDesc.Add(levelCommandsCursor);
    }


    private void registerLv1Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<WhatDropsFromCommand>("whatdropsfrom", 1);
        addCommand<WhoDropsCommand>("whodrops", 1);
        addCommand<BuffMeCommand>("buffme", 1);
        addCommand<GotoCommand>("goto", 1);

        commandsNameDesc.Add(levelCommandsCursor);
    }


    private void registerLv2Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<RechargeCommand>("recharge", 2);
        addCommand<WhereaMiCommand>("whereami", 2);
        addCommand<HideCommand>("hide", 2);
        addCommand<UnHideCommand>("unhide", 2);
        addCommand<SpCommand>("sp", 2);
        addCommand<ApCommand>("ap", 2);
        addCommand<EmpowerMeCommand>("empowerme", 2);
        addCommand<BuffMapCommand>("buffmap", 2);
        addCommand<BuffCommand>("buff", 2);
        addCommand<BombCommand>("bomb", 2);
        addCommand<DcCommand>("dc", 2);
        addCommand<ClearDropsCommand>("cleardrops", 2);
        addCommand<ClearSlotCommand>("clearslot", 2);
        addCommand<ClearSavedLocationsCommand>("clearsavelocs", 2);
        addCommand<WarpCommand>("warp", 2);
        addCommand<SummonCommand>(new string[] { "warphere", "summon" }, 2);
        addCommand<ReachCommand>(new string[] { "warpto", "reach", "follow" }, 2);
        addCommand<GmShopCommand>("gmshop", 2);
        addCommand<HealCommand>("heal", 2);
        addCommand<ItemCommand>("item", 2);
        addCommand<ItemDropCommand>("drop", 2);
        addCommand<LevelCommand>("level", 2);
        addCommand<LevelProCommand>("levelpro", 2);
        addCommand<SetSlotCommand>("setslot", 2);
        addCommand<SetStatCommand>("setstat", 2);
        addCommand<MaxStatCommand>("maxstat", 2);
        addCommand<MaxSkillCommand>("maxskill", 2);
        addCommand<ResetSkillCommand>("resetskill", 2);
        addCommand<SearchCommand>("search", 2);
        addCommand<JailCommand>("jail", 2);
        addCommand<UnJailCommand>("unjail", 2);
        addCommand<JobCommand>("job", 2);
        addCommand<UnBugCommand>("unbug", 2);
        addCommand<IdCommand>("id", 2);
        addCommand<GachaListCommand>("gachalist");
        addCommand<LootCommand>("loot");

        commandsNameDesc.Add(levelCommandsCursor);
    }

    private void registerLv3Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<DebuffCommand>("debuff", 3);
        addCommand<FlyCommand>("fly", 3);
        addCommand<SpawnCommand>("spawn", 3);
        addCommand<MuteMapCommand>("mutemap", 3);
        addCommand<CheckDmgCommand>("checkdmg", 3);
        addCommand<InMapCommand>("inmap", 3);
        addCommand<ReloadEventsCommand>("reloadevents", 3);
        addCommand<ReloadDropsCommand>("reloaddrops", 3);
        addCommand<ReloadPortalsCommand>("reloadportals", 3);
        addCommand<ReloadMapCommand>("reloadmap", 3);
        addCommand<ReloadShopsCommand>("reloadshops", 3);
        addCommand<HpMpCommand>("hpmp", 3);
        addCommand<MaxHpMpCommand>("maxhpmp", 3);
        addCommand<MusicCommand>("music", 3);
        addCommand<MonitorCommand>("monitor", 3);
        addCommand<MonitorsCommand>("monitors", 3);
        addCommand<IgnoreCommand>("ignore", 3);
        addCommand<IgnoredCommand>("ignored", 3);
        addCommand<PosCommand>("pos", 3);
        addCommand<ToggleCouponCommand>("togglecoupon", 3);
        addCommand<ChatCommand>("togglewhitechat", 3);
        addCommand<FameCommand>("fame", 3);
        addCommand<GiveNxCommand>("givenx", 3);
        addCommand<GiveVpCommand>("givevp", 3);
        addCommand<GiveMesosCommand>("givems", 3);
        addCommand<GiveRpCommand>("giverp", 3);
        addCommand<ExpedsCommand>("expeds", 3);
        addCommand<KillCommand>("kill", 3);
        addCommand<SeedCommand>("seed", 3);
        addCommand<MaxEnergyCommand>("maxenergy", 3);
        addCommand<KillAllCommand>("killall", 3);
        addCommand<NoticeCommand>("notice", 3);
        addCommand<RipCommand>("rip", 3);
        addCommand<OpenPortalCommand>("openportal", 3);
        addCommand<ClosePortalCommand>("closeportal", 3);
        addCommand<PeCommand>("pe", 3);
        addCommand<StartEventCommand>("startevent", 3);
        addCommand<EndEventCommand>("endevent", 3);
        addCommand<StartMapEventCommand>("startmapevent", 3);
        addCommand<StopMapEventCommand>("stopmapevent", 3);
        addCommand<OnlineTwoCommand>("online2", 3);
        addCommand<BanCommand>("ban", 3);
        addCommand<UnBanCommand>("unban", 3);
        addCommand<HealMapCommand>("healmap", 3);
        addCommand<HealPersonCommand>("healperson", 3);
        addCommand<HurtCommand>("hurt", 3);
        addCommand<KillMapCommand>("killmap", 3);
        addCommand<NightCommand>("night", 3);
        addCommand<NpcCommand>("npc", 3);
        addCommand<FaceCommand>("face", 3);
        addCommand<HairCommand>("hair", 3);
        addCommand<QuestStartCommand>("startquest", 3);
        addCommand<QuestCompleteCommand>("completequest", 3);
        addCommand<QuestResetCommand>("resetquest", 3);
        addCommand<TimerCommand>("timer", 3);
        addCommand<TimerMapCommand>("timermap", 3);
        addCommand<TimerAllCommand>("timerall", 3);
        addCommand<WarpMapCommand>("warpmap", 3);
        addCommand<WarpAreaCommand>("warparea", 3);

        commandsNameDesc.Add(levelCommandsCursor);
    }

    private void registerLv4Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<ServerMessageCommand>("servermessage", 4);
        addCommand<ProItemCommand>("proitem", 4);
        addCommand<SetEqStatCommand>("seteqstat", 4);
        addCommand<ExpRateCommand>("exprate", 4);
        addCommand<MesoRateCommand>("mesorate", 4);
        addCommand<DropRateCommand>("droprate", 4);
        addCommand<BossDropRateCommand>("bossdroprate", 4);
        addCommand<QuestRateCommand>("questrate", 4);
        addCommand<TravelRateCommand>("travelrate", 4);
        addCommand<FishingRateCommand>("fishrate", 4);
        addCommand<ItemVacCommand>("itemvac", 4);
        addCommand<ForceVacCommand>("forcevac", 4);
        addCommand<ZakumCommand>("zakum", 4);
        addCommand<HorntailCommand>("horntail", 4);
        addCommand<PinkbeanCommand>("pinkbean", 4);
        addCommand<PapCommand>("pap", 4);
        addCommand<PianusCommand>("pianus", 4);
        addCommand<CakeCommand>("cake", 4);
        addCommand<PlayerNpcCommand>("playernpc", 4);
        addCommand<PlayerNpcRemoveCommand>("playernpcremove", 4);
        addCommand<PnpcCommand>("pnpc", 4);
        addCommand<PnpcRemoveCommand>("pnpcremove", 4);
        addCommand<PmobCommand>("pmob", 4);
        addCommand<PmobRemoveCommand>("pmobremove", 4);

        commandsNameDesc.Add(levelCommandsCursor);
    }

    private void registerLv5Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<DebugCommand>("debug", 5);
        addCommand<SetCommand>("set", 5);
        addCommand<ShowPacketsCommand>("showpackets", 5);
        addCommand<ShowMoveLifeCommand>("showmovelife", 5);
        addCommand<ShowSessionsCommand>("showsessions", 5);
        addCommand<IpListCommand>("iplist", 5);

        commandsNameDesc.Add(levelCommandsCursor);
    }

    private void registerLv6Commands()
    {
        levelCommandsCursor = new KeyValuePair<List<string>, List<string>>(new(), new());

        addCommand<SetGmLevelCommand>("setgmlevel", 6);
        addCommand<WarpWorldCommand>("warpworld", 6);
        addCommand<SaveAllCommand>("saveall", 6);
        addCommand<DCAllCommand>("dcall", 6);
        addCommand<MapPlayersCommand>("mapplayers", 6);
        addCommand<GetAccCommand>("getacc", 6);
        addCommand<ShutdownCommand>("shutdown", 6);
        addCommand<ClearQuestCacheCommand>("clearquestcache", 6);
        addCommand<ClearQuestCommand>("clearquest", 6);
        addCommand<SupplyRateCouponCommand>("supplyratecoupon", 6);
        addCommand<SpawnAllPNpcsCommand>("spawnallpnpcs", 6);
        addCommand<EraseAllPNpcsCommand>("eraseallpnpcs", 6);
        addCommand<ServerAddChannelCommand>("addchannel", 6);
        addCommand<ServerAddWorldCommand>("addworld", 6);
        addCommand<ServerRemoveChannelCommand>("removechannel", 6);
        addCommand<ServerRemoveWorldCommand>("removeworld", 6);

        commandsNameDesc.Add(levelCommandsCursor);
    }

}
