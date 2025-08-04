/*
    This file is part of the HeavenMS MapleStory Server
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


using Application.Core.ServerTransports;
using Application.Core.Tools;
using Application.Shared.Items;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Core.Channel.DataProviders;

/**
 * @author RonanLana
 */

/**
 * Only used in 1 script that gives players information about where skillbooks can be found
 */
public class SkillbookInformationProvider : DataBootstrap
{
    private volatile Dictionary<int, SkillBookEntry> foundSkillbooks = new();

    readonly IChannelServerTransport _transport;

    public SkillbookInformationProvider(IChannelServerTransport transport, ILogger<DataBootstrap> logger) : base(logger)
    {
        Name = "能手册";
        _transport = transport;
    }

    protected override void LoadDataInternal()
    {
        Stopwatch sw = new Stopwatch();
        Dictionary<int, SkillBookEntry> loadedSkillbooks = new();
        sw.Start();
        loadedSkillbooks.putAll(fetchSkillbooksFromQuests());
        _logger.LogDebug("fetchSkillbooksFromQuests 耗时 {Cost}s", sw.Elapsed.TotalSeconds);
        sw.Restart();
        loadedSkillbooks.putAll(fetchSkillbooksFromReactors());
        _logger.LogDebug("fetchSkillbooksFromReactors 耗时 {Cost}s", sw.Elapsed.TotalSeconds);
        sw.Restart();
        loadedSkillbooks.putAll(fetchSkillbooksFromScripts());
        _logger.LogDebug("fetchSkillbooksFromScripts 耗时 {Cost}s", sw.Elapsed.TotalSeconds);
        foundSkillbooks = loadedSkillbooks;
    }

    private static bool is4thJobSkill(int itemid)
    {
        return itemid / 10000 % 10 == 2;
    }

    private static bool isSkillBook(int itemid)
    {
        return itemid >= ItemId.SKILLBOOK_MIN_ITEMID && itemid < ItemId.SKILLBOOK_MAX_ITEMID;
    }

    private static bool isQuestBook(int itemid)
    {
        return itemid >= 4001107 && itemid <= 4001114 || itemid >= 4161015 && itemid <= 4161023;
    }

    private static int fetchQuestbook(Data checkData, string quest)
    {
        var questStartData = checkData.getChildByPath(quest)?.getChildByPath("0");

        var startReqItemData = questStartData?.getChildByPath("item");
        if (startReqItemData != null)
        {
            foreach (Data itemData in startReqItemData.getChildren())
            {
                int itemId = DataTool.getInt("id", itemData, 0);
                if (isQuestBook(itemId))
                {
                    return itemId;
                }
            }
        }

        var startReqQuestData = questStartData?.getChildByPath("quest");
        if (startReqQuestData != null)
        {
            HashSet<int> reqQuests = new();

            foreach (Data questStatusData in startReqQuestData.getChildren())
            {
                int reqQuest = DataTool.getInt("id", questStatusData, 0);
                if (reqQuest > 0)
                {
                    reqQuests.Add(reqQuest);
                }
            }

            foreach (int reqQuest in reqQuests)
            {
                int book = fetchQuestbook(checkData, reqQuest.ToString());
                if (book > -1)
                {
                    return book;
                }
            }
        }

        return -1;
    }

    private static Dictionary<int, SkillBookEntry> fetchSkillbooksFromQuests()
    {
        DataProvider questDataProvider = DataProviderFactory.getDataProvider(WZFiles.QUEST);
        Data actData = questDataProvider.getData("Act.img");
        Data checkData = questDataProvider.getData("Check.img");

        Dictionary<int, SkillBookEntry> loadedSkillbooks = new();
        foreach (Data questData in actData.getChildren())
        {
            foreach (Data questStatusData in questData.getChildren())
            {
                foreach (Data questNodeData in questStatusData.getChildren())
                {
                    var actNodeName = questNodeData.getName();
                    if (actNodeName == "item")
                    {
                        foreach (Data questItemData in questNodeData.getChildren())
                        {
                            int itemId = DataTool.getInt("id", questItemData, 0);
                            int itemCount = DataTool.getInt("count", questItemData, 0);

                            if (isSkillBook(itemId) && itemCount > 0)
                            {
                                int questbook = fetchQuestbook(checkData, questData.getName());
                                if (questbook < 0)
                                {
                                    loadedSkillbooks.TryAdd(itemId, SkillBookEntry.QUEST);
                                }
                                else
                                {
                                    loadedSkillbooks.TryAdd(itemId, SkillBookEntry.QUEST_BOOK);
                                }
                            }
                        }
                    }
                    else if (actNodeName == "skill")
                    {
                        foreach (Data questSkillData in questNodeData.getChildren())
                        {
                            int skillId = DataTool.getInt("id", questSkillData, 0);
                            if (is4thJobSkill(skillId))
                            {
                                // negative itemids are skill rewards

                                int questbook = fetchQuestbook(checkData, questData.getName());
                                if (questbook < 0)
                                {
                                    loadedSkillbooks.TryAdd(-skillId, SkillBookEntry.QUEST_REWARD);
                                }
                                else
                                {
                                    loadedSkillbooks.TryAdd(-skillId, SkillBookEntry.QUEST_BOOK);
                                }
                            }
                        }
                    }
                }
            }
        }

        return loadedSkillbooks;
    }

    private Dictionary<int, SkillBookEntry> fetchSkillbooksFromReactors()
    {
        Dictionary<int, SkillBookEntry> loadedSkillbooks = new();

        try
        {
            var dataList = _transport.RequestReactorSkillBooks();

            foreach (var item in dataList)
            {
                loadedSkillbooks.TryAdd(item, SkillBookEntry.REACTOR);
            }
        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }

        return loadedSkillbooks;
    }


    private static HashSet<int> findMatchingSkillbookIdsOnFile(string fileContent)
    {
        HashSet<int> matches = new HashSet<int>();

        Regex regex = new Regex("(?<!\\d)(22(8|9)[0-9]{4})(?!\\d)");
        MatchCollection matchCollection = regex.Matches(fileContent);

        foreach (Match match in matchCollection)
        {
            matches.Add(int.Parse(match.Groups[1].Value));
        }

        return matches;
    }

    private static string readFileToString(FileInfo file, string encoding = "UTF-8")
    {
        return File.ReadAllText(file.FullName, Encoding.GetEncoding(encoding));
    }

    private Dictionary<int, SkillBookEntry> fileSearchMatchingData(FileInfo file)
    {
        Dictionary<int, SkillBookEntry> scriptFileSkillbooks = new();

        try
        {
            string fileContent = readFileToString(file, "UTF-8");

            HashSet<int> skillbookIds = findMatchingSkillbookIdsOnFile(fileContent);
            foreach (int skillbookId in skillbookIds)
            {
                scriptFileSkillbooks.AddOrUpdate(skillbookId, SkillBookEntry.SCRIPT);
            }
        }
        catch (IOException ioe)
        {
            _logger.LogError(ioe, "Failed to read file:{FileName}", file.FullName);
        }

        return scriptFileSkillbooks;
    }

    private Dictionary<int, SkillBookEntry> fetchSkillbooksFromScripts()
    {
        return FileCache.GetOrCreate("skillbookinformation_script", () =>
        {
            Dictionary<int, SkillBookEntry> scriptSkillbooks = new();

            foreach (var file in ScriptResFactory.LoadAllScript())
            {
                if (file.Name.EndsWith(".js"))
                {
                    scriptSkillbooks.putAll(fileSearchMatchingData(file));
                }
            }
            return scriptSkillbooks;
        }) ?? [];
    }

    public SkillBookEntry getSkillbookAvailability(int itemId)
    {
        return foundSkillbooks.GetValueOrDefault(itemId, SkillBookEntry.UNAVAILABLE);
    }

    public List<int> getTeachableSkills(IPlayer chr)
    {
        List<int> list = new();

        foreach (int book in foundSkillbooks.Keys)
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

}
