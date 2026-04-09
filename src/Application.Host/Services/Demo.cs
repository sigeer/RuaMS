using Application.Core.Game.Life;
using Application.Scripting;
using Application.Templates.Etc;
using Application.Templates.Map;
using Application.Templates.Npc;
using Application.Templates.Providers;
using Application.Templates.Reactor;
using Application.Templates.XmlWzReader.Provider;
using client.inventory;
using Microsoft.CSharp;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Host.Services
{
    public class Demo
    {
        static string GetLines(IEnumerable<(string header, string content)> lines)
        {
            StringBuilder sb = new();
            foreach (var item in lines)
            {
                sb.Append("// ").AppendLine(item.header)
                    .AppendLine(item.content);

            }
            return sb.ToString();

        }
        static bool IsValidMethodName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            // 使用 C# 语言提供程序
            using (var provider = new CSharpCodeProvider())
            {
                return provider.IsValidIdentifier(name);
            }
        }
        public static void ExportReactorAct()
        {
            var allWzData = ProviderSource.Instance.GetProvider<ReactorProvider>()
                .LoadAll().OfType<ReactorTemplate>().Where(x => !string.IsNullOrEmpty(x.Action)).Select(y => new { ReactorId = y.TemplateId, y.Action })
                .GroupBy(x => x.Action).ToDictionary(x => x.Key, x => x.ToList());

            var allExsitedScripts = ScriptSource.Instance.GetSubScriptsPath("reactor")
                .ToArray()
                .ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => GetFunctionBody("act", File.ReadAllText(x), "rm."));

            var content = allWzData.Select(x => $$"""
                    // Reactor: {{string.Join(", ", x.Value.Select(y => y.ReactorId))}} {{(!IsValidMethodName(x.Key) ? (Environment.NewLine + "[ScriptName(\"" + x.Key + "\")]") : "")}}
                    public Task {{(!IsValidMethodName(x.Key) ? "s_" + x.Key : x.Key)}}()
                    {
                        // TODO
                        {{GetLines(x.Value.Select(y => (y.ReactorId.ToString(), allExsitedScripts?.GetValueOrDefault(y.ReactorId.ToString()))))}}
                        return Task.CompletedTask;
                    }
                    {{Environment.NewLine}}
            """);
            File.WriteAllText(
                    "ReactorActScript.cs",
                    $$"""
                            using Application.Core.Client;
                            using Application.Core.Game.Maps;
                            using scripting.reactor;
                            using server.maps;

                            namespace Application.Plugin.Script
                            {
                                // Extra: {{string.Join(", ", allExsitedScripts.Keys.Except(allWzData.Values.SelectMany(x => x.Select(y => y.ReactorId.ToString()))))}}
                                internal class ReactorActScript : ReactorActionManager
                                {
                                    public ReactorActScript(IChannelClient c, Reactor r) : base(c, r)
                                    {
                                    }

                                    {{string.Join(Environment.NewLine, content)}}

                                }
                            }
                            """);
        }

        public static void ExportReactorHit()
        {
            var allWzData = ProviderSource.Instance.GetProvider<ReactorProvider>()
                .LoadAll().OfType<ReactorTemplate>().Where(x => !string.IsNullOrEmpty(x.Action)).Select(y => new { ReactorId = y.TemplateId, y.Action })
                .GroupBy(x => x.Action).ToDictionary(x => x.Key, x => x.ToList());

            var allExsitedScripts = ScriptSource.Instance.GetSubScriptsPath("reactor")
                .ToArray()
                .ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => GetFunctionBody("hit", File.ReadAllText(x), "rm."));

            var content = allWzData.Select(x => $$"""
                    // Reactor: {{string.Join(", ", x.Value.Select(y => y.ReactorId))}} {{(!IsValidMethodName(x.Key) ? (Environment.NewLine + "[ScriptName(\"" + x.Key + "\")]") : "")}}
                    public Task {{(!IsValidMethodName(x.Key) ? "s_" + x.Key : x.Key)}}()
                    {
                        // TODO
                        {{GetLines(x.Value.Select(y => (y.ReactorId.ToString(), allExsitedScripts?.GetValueOrDefault(y.ReactorId.ToString()))))}}
                        return Task.CompletedTask;
                    }
                    {{Environment.NewLine}}
            """);
            File.WriteAllText(
                    "ReactorHitScript.cs",
                    $$"""
                            using Application.Core.Client;
                            using Application.Core.Game.Maps;
                            using scripting.reactor;
                            using server.maps;

                            namespace Application.Plugin.Script
                            {
                                // Extra: {{string.Join(", ", allExsitedScripts.Keys.Except(allWzData.Values.SelectMany(x => x.Select(y => y.ReactorId.ToString()))))}}
                                internal class ReactorHitScript : ReactorActionManager
                                {
                                    public ReactorHitScript(IChannelClient c, Reactor r) : base(c, r)
                                    {
                                    }

                                    {{string.Join(Environment.NewLine, content)}}

                                }
                            }
                            """);
        }

        static string GetFunctionBody(string functionName, string code, string innerObj)
        {
            // 1. 定位函数定义的起始位置
            string pattern = $@"function\s+{functionName}\s*\([^)]*\)\s*\{{";
            Regex startRegex = new Regex(pattern);
            Match startMatch = startRegex.Match(code);
            if (!startMatch.Success) return null;

            int startIndex = startMatch.Index + startMatch.Length; // 跳过 '{'
            int braceCount = 1;
            int endIndex = startIndex;

            // 2. 逐字符扫描，找到匹配的结束 '}'
            for (int i = startIndex; i < code.Length && braceCount > 0; i++)
            {
                if (code[i] == '{') braceCount++;
                else if (code[i] == '}') braceCount--;
                if (braceCount == 0)
                {
                    endIndex = i;
                    break;
                }
            }

            if (braceCount != 0) return null; // 未找到匹配的 }

            // 3. 提取函数体（不包括外层的花括号）
            string body = code.Substring(startIndex, endIndex - startIndex);
            return body.Trim()
                .Replace(innerObj, "")
                .Replace("return true", "return Task.FromResult(true)")
                .Replace("return false", "return Task.FromResult(false)");
        }
        public static void ExportPortal()
        {
            var allWzData = ProviderSource.Instance.GetProvider<MapProvider>()
                .LoadAll().OfType<MapTemplate>().SelectMany(x => x.Portals.Where(y => y.Script != null).Select(y => new { MapId = x.TemplateId, y.Script }))
                .GroupBy(x => x.Script);

            var allExsitedPortalScripts = ScriptSource.Instance.GetSubScriptsPath("portal").ToArray();

            var content = allWzData.Select(x => $$"""
                    // Map: {{string.Join(", ", x.Select(y => y.MapId))}}
                    public Task<bool> {{x.Key}}()
                    {
                        // TODO
                        return Task.FromResult(true);
                    }
                    {{Environment.NewLine}}
            """);
            File.WriteAllText(
                    "PortalScript.cs",
                    $$"""
                            using Application.Core.Client;
                            using Application.Scripting;
                            using Humanizer;
                            using scripting.portal;
                            using server.maps;

                            namespace Application.Plugin.Script
                            {
                                internal class PortalScript : PortalPlayerInteraction
                                {
                                    public PortalScript(IChannelClient c, Portal p) : base(c, p)
                                    {
                                    }

                                    {{string.Join(Environment.NewLine, content)}}

                                }
                            }
                            """);
        }

        public static void ExportMap()
        {
            var allWzData = ProviderSource.Instance.GetProvider<MapProvider>()
                .LoadAll().OfType<MapTemplate>().Where(x => !string.IsNullOrEmpty(x.OnUserEnter)).Select(y => new { MapId = y.TemplateId, y.OnUserEnter })
                .GroupBy(x => x.OnUserEnter).ToDictionary(x => x.Key, x => x.ToList());

            var allExsitedScripts = ScriptSource.Instance.GetSubScriptsPath("map\\onUserEnter")
                .ToArray()
                .ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => GetFunctionBody("start", File.ReadAllText(x), "ms."));

            var content = allWzData.Select(x => $$"""
                    // Map: {{string.Join(", ", x.Value.Select(y => y.MapId))}} {{(char.IsNumber(x.Key[0]) ? (Environment.NewLine + "[ScriptName(\"" + x.Key + "\")]") : "")}}
                    public Task {{(char.IsNumber(x.Key[0]) ? "s_" + x.Key : x.Key)}}()
                    {
                        // TODO
                        {{allExsitedScripts?.GetValueOrDefault(x.Key)}}
                        return Task.CompletedTask;
                    }
                    {{Environment.NewLine}}
            """);
            File.WriteAllText(
                    "MapEnterScript.cs",
                    $$"""
                            using Application.Core.Client;
                            using Application.Core.Game.Maps;
                            using scripting.map;

                            namespace Application.Plugin.Script
                            {
                                // Extra: {{string.Join(", ", allExsitedScripts.Keys.Except(allWzData.Keys))}}
                                internal class MapEnterScript : MapScriptMethods
                                {
                                    public MapEnterScript(IChannelClient c, IMap m) : base(c, m)
                                    {
                                    }

                                    {{string.Join(Environment.NewLine, content)}}

                                }
                            }
                            """);
        }

        public static void ExportNpc()
        {
            var allWzData = ProviderSource.Instance.GetProvider<NpcProvider>()
                .LoadAll().OfType<NpcTemplate>().Where(x => !string.IsNullOrEmpty(x.Script)).Select(y => new { NpcId =y.TemplateId, y.Script })
                .GroupBy(x => x.Script).ToDictionary(x => x.Key, x => x.ToList());

            var allExsitedScripts = ScriptSource.Instance.GetSubScripts("npc")
                .ToArray();


            var content = allWzData.Select(x => $$"""
                    // Npc: {{string.Join(", ", x.Value.Select(y => y.NpcId ))}} {{(!IsValidMethodName(x.Key) ? (Environment.NewLine + "[ScriptName(\"" + x.Key + "\")]") : "")}}
                    public Task {{(!IsValidMethodName(x.Key) ? "s_" + x.Key : x.Key)}}()
                    {
                        // TODO
                        return Task.CompletedTask;
                    }
                    {{Environment.NewLine}}
            """);
            File.WriteAllText(
                    "NpcScript.cs",
                    $$"""
                            using Application.Core.Client;
                            using Application.Scripting;
                            using Humanizer;
                            using scripting.npc;

                            namespace Application.Plugin.Script
                            {
                                // 未包含的NPC: {{string.Join(", ", allExsitedScripts.Except(allWzData.Values.SelectMany(x => x.Select(y => y.NpcId.ToString()))))}}
                                internal class NpcScript : NPCConversationManager
                                {
                                    public NpcScript(IChannelClient c, int npc, int npcOId) : base(c, npc, npcOId, null)
                                    {
                                    }

                                    {{string.Join(Environment.NewLine, content)}}

                                }
                            }
                            """);
        }
    }
}
