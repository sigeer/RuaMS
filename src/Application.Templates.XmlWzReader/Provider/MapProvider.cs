using Application.Templates.Exceptions;
using Application.Templates.Map;
using Application.Templates.Providers;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class MapProvider : AbstractGroupProvider<MapTemplate>
    {
        public override string ProviderName => ProviderNames.Map;

        public MapProvider(TemplateOptions options)
            : base(options)
        {
            _files = Directory.GetFiles(Path.Combine(_dataBaseDir, ProviderName, "Map"), "*.xml", SearchOption.AllDirectories)
                    .Select(x => Path.GetRelativePath(_dataBaseDir, x))
                    .ToArray();
        }

        protected override string? GetImgPathByTemplateId(int mapId)
        {
            var mapArea = $"Map{mapId / 100000000}";
            string fileName = mapId.ToString().PadLeft(9, '0') + ".img.xml";
            return Path.Combine(ProviderName, "Map", mapArea, fileName);
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                using var fis = _fileProvider.ReadFile(imgPath);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 9), out var mapId))
                    throw new TemplateFormatException(ProviderName, imgPath);

                var mapTemplate = new MapTemplate(mapId);
                foreach (var item in xDoc.Elements())
                {
                    var nodeType = item.GetName();
                    if (nodeType == "info")
                    {
                        foreach (var infoPropNode in item.Elements())
                        {
                            var name = infoPropNode.GetName();
                            if (name == "link")
                                GetItem(infoPropNode.GetIntValue())?.CloneLink(mapTemplate);
                            else if (name == "forcedReturn")
                                mapTemplate.ForcedReturn = infoPropNode.GetIntValue();
                            else if (name == "returnMap")
                                mapTemplate.ReturnMap = infoPropNode.GetIntValue();
                            else if (name == "fly")
                                mapTemplate.FlyMap = infoPropNode.GetBoolValue();
                            else if (name == "town")
                                mapTemplate.Town = infoPropNode.GetBoolValue();
                            else if (name == "everlast")
                                mapTemplate.Everlast = infoPropNode.GetBoolValue();
                            else if (name == "createMobInterval")
                                mapTemplate.CreateMobInterval = infoPropNode.GetIntValue();
                            else if (name == "mobRate")
                                mapTemplate.MobRate = infoPropNode.GetFloatValue();

                            else if (name == "onFirstUserEnter")
                                mapTemplate.OnFirstUserEnter = infoPropNode.GetStringValue();
                            else if (name == "onUserEnter")
                                mapTemplate.OnUserEnter = infoPropNode.GetStringValue();

                            else if (name == "VRRight")
                                mapTemplate.VRRight = infoPropNode.GetIntValue();
                            else if (name == "VRBottom")
                                mapTemplate.VRBottom = infoPropNode.GetIntValue();
                            else if (name == "VRLeft")
                                mapTemplate.VRLeft = infoPropNode.GetIntValue();
                            else if (name == "VRTop")
                                mapTemplate.VRTop = infoPropNode.GetIntValue();

                            else if (name == "decHP")
                                mapTemplate.DecHP = infoPropNode.GetIntValue();
                            else if (name == "decInterval")
                                mapTemplate.DecInterval = infoPropNode.GetIntValue();
                            else if (name == "protectItem")
                                mapTemplate.ProtectItem = infoPropNode.GetIntValue();
                            else if (name == "timeLimit")
                                mapTemplate.TimeLimit = infoPropNode.GetIntValue();
                            else if (name == "fieldLimit")
                                mapTemplate.FieldLimit = infoPropNode.GetIntValue();
                            else if (name == "fieldType")
                                mapTemplate.FieldType = infoPropNode.GetIntValue();

                            else if (name == "recovery")
                                mapTemplate.RecoveryRate = infoPropNode.GetFloatValue();
                            else if (name == "fixedMobCapacity")
                                mapTemplate.FixedMobCapacity = infoPropNode.GetIntValue();

                            else if (nodeType == "timeMob")
                            {
                                var timeMob = new MapTimeMobTemplate();
                                foreach (var timeMobProp in infoPropNode.Elements())
                                {
                                    var timeMobPropName = timeMobProp.GetName();
                                    if (timeMobPropName == "id")
                                        timeMob.Id = timeMobProp.GetIntValue();
                                    else if (timeMobPropName == "message")
                                        timeMob.Message = timeMobProp.GetStringValue() ?? "";
                                    else if (timeMobPropName == "startHour")
                                        timeMob.StartHour = timeMobProp.GetIntValue();
                                    else if (timeMobPropName == "endHour")
                                        timeMob.EndHour = timeMobProp.GetIntValue();
                                }
                                mapTemplate.TimeMob = timeMob;
                            }
                        }
                    }

                    else if (nodeType == "shipObj")
                        mapTemplate.HasShip = true;
                    else if (nodeType == "clock")
                        mapTemplate.HasClock = true;

                    else if (nodeType == "monsterCarnival")
                        ProcessMC(mapTemplate, item);
                    else if (nodeType == "snowball")
                        ProcessSnowball(mapTemplate, item);
                    else if (nodeType == "coconut")
                        ProcessCoconut(mapTemplate, item);

                    else if (nodeType == "foothold")
                    {
                        ProcessMapFoothold(mapTemplate, item);
                    }

                    else if (nodeType == "back")
                    {
                        ProcessMapBack(mapTemplate, item);
                    }

                    else if (nodeType == "portal")
                    {
                        ProcessMapPortal(mapTemplate, item);
                    }

                    else if (nodeType == "life")
                    {
                        ProcessMapLife(mapTemplate, item);
                    }

                    else if (nodeType == "reactor")
                    {
                        ProcessMapReactor(mapTemplate, item);
                    }

                    else if (nodeType == "miniMap")
                    {
                        var mini = new MapMiniMapTemplate();
                        foreach (var timeMobProp in item.Elements())
                        {
                            var timeMobPropName = timeMobProp.GetName();
                            if (timeMobPropName == "width")
                                mini.Width = timeMobProp.GetIntValue();
                            else if (timeMobPropName == "height")
                                mini.Height = timeMobProp.GetIntValue();
                            else if (timeMobPropName == "centerX")
                                mini.CenterX = timeMobProp.GetIntValue();
                            else if (timeMobPropName == "centerY")
                                mini.CenterY = timeMobProp.GetIntValue();
                        }
                        mapTemplate.MiniMap = mini;
                    }

                    else if (nodeType == "area")
                    {
                        var list = new List<MapAreaTemplate>();
                        foreach (var areaItem in item.Elements())
                        {
                            var model = new MapAreaTemplate();
                            foreach (var areaProp in areaItem.Elements())
                            {
                                var areaPropName = areaProp.GetName();
                                if (areaPropName == "x1")
                                    model.X1 = areaProp.GetIntValue();
                                else if (areaPropName == "x2")
                                    model.X2 = areaProp.GetIntValue();
                                else if (areaPropName == "y1")
                                    model.Y1 = areaProp.GetIntValue();
                                else if (areaPropName == "y2")
                                    model.Y2 = areaProp.GetIntValue();
                            }
                        }
                        mapTemplate.Areas = list.ToArray();
                    }

                    else if (nodeType == "seat")
                    {
                        mapTemplate.SeatCount = item.Elements().Count();
                    }
                }
                InsertItem(mapTemplate);
                return [mapTemplate];
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private static void ProcessMapBack(MapTemplate mapTemplate, XElement item)
        {
            var list = new List<MapBackTemplate>();
            foreach (var backItem in item.Elements())
            {
                var model = new MapBackTemplate() { Index = backItem.GetIntName() };
                foreach (var backProp in backItem.Elements())
                {
                    var name = backProp.GetName();
                    if (name == "type")
                    {
                        model.Type = backProp.GetIntValue();
                        break;
                    }
                }
                list.Add(model);
            }
            mapTemplate.Backs = list.ToArray();
        }

        private static void ProcessMapFoothold(MapTemplate mapTemplate, XElement item)
        {
            var list = new List<MapFootholdTemplate>();
            foreach (var fhList in item.Elements())
            {
                foreach (var fhList2 in fhList.Elements())
                {
                    foreach (var fhItemNode in fhList2.Elements())
                    {
                        var fhItem = new MapFootholdTemplate() { Index = fhItemNode.GetIntName() };
                        foreach (var fhProp in fhItemNode.Elements())
                        {
                            var name = fhProp.GetName();
                            if (name == "next")
                                fhItem.Next = fhProp.GetIntValue();
                            else if (name == "prev")
                                fhItem.Prev = fhProp.GetIntValue();
                            else if (name == "x1")
                                fhItem.X1 = fhProp.GetIntValue();
                            else if (name == "x2")
                                fhItem.X2 = fhProp.GetIntValue();
                            else if (name == "y1")
                                fhItem.Y1 = fhProp.GetIntValue();
                            else if (name == "y2")
                                fhItem.Y2 = fhProp.GetIntValue();
                        }
                        list.Add(fhItem);
                    }
                }
            }
            mapTemplate.Footholds = list.ToArray();
        }

        private static void ProcessMapPortal(MapTemplate mapTemplate, XElement item)
        {
            var list = new List<MapPortalTemplate>();
            foreach (var portalItem in item.Elements())
            {
                var model = new MapPortalTemplate() { nIndex = portalItem.GetIntName() };
                foreach (var portalProp in portalItem.Elements())
                {
                    var name = portalProp.GetName();
                    if (name == "pn")
                        model.sPortalName = portalProp.GetStringValue();
                    else if (name == "tn")
                        model.sTargetName = portalProp.GetStringValue();
                    else if (name == "script")
                        model.Script = portalProp.GetStringValue();
                    else if (name == "pt")
                        model.nPortalType = portalProp.GetIntValue();
                    else if (name == "tm")
                        model.nTargetMap = portalProp.GetIntValue();
                    else if (name == "x")
                        model.nX = portalProp.GetIntValue();
                    else if (name == "y")
                        model.nY = portalProp.GetIntValue();
                }
                list.Add(model);
            }
            mapTemplate.Portals = list.ToArray();
        }

        private static void ProcessMapReactor(MapTemplate mapTemplate, XElement item)
        {
            var list = new List<MapReactorTemplate>();
            foreach (var reactorItem in item.Elements())
            {
                var model = new MapReactorTemplate() { Index = reactorItem.GetIntName() };
                foreach (var reactorProp in reactorItem.Elements())
                {
                    var name = reactorProp.GetName();
                    if (name == "name")
                        model.Name = reactorProp.GetStringValue();
                    else if (name == "id")
                        model.Id = reactorProp.GetIntValue();
                    else if (name == "f")
                        model.F = reactorProp.GetIntValue();
                    else if (name == "reactorTime")
                        model.ReactorTime = reactorProp.GetIntValue();
                    else if (name == "x")
                        model.X = reactorProp.GetIntValue();
                    else if (name == "y")
                        model.Y = reactorProp.GetIntValue();
                }
                list.Add(model);
            }
            mapTemplate.Reactors = list.ToArray();
        }

        private static void ProcessMapLife(MapTemplate mapTemplate, XElement item)
        {
            var list = new List<MapLifeTemplate>();
            foreach (var lifeItem in item.Elements())
            {
                var model = new MapLifeTemplate() { Index = lifeItem.GetIntName() };
                foreach (var lifeProp in lifeItem.Elements())
                {
                    var name = lifeProp.GetName();
                    if (name == "id")
                        model.Id = lifeProp.GetIntValue();
                    else if (name == "cy")
                        model.CY = lifeProp.GetIntValue();
                    else if (name == "hide")
                        model.Hide = lifeProp.GetBoolValue();
                    else if (name == "f")
                        model.F = lifeProp.GetIntValue();
                    else if (name == "fh")
                        model.Foothold = lifeProp.GetIntValue();
                    else if (name == "mobTime")
                        model.MobTime = lifeProp.GetIntValue();
                    else if (name == "type")
                        model.Type = lifeProp.GetStringValue() ?? "";
                    else if (name == "rx0")
                        model.RX0 = lifeProp.GetIntValue();
                    else if (name == "rx1")
                        model.RX1 = lifeProp.GetIntValue();
                    else if (name == "x")
                        model.X = lifeProp.GetIntValue();
                    else if (name == "y")
                        model.Y = lifeProp.GetIntValue();
                    else if (name == "team")
                        model.Team = lifeProp.GetIntValue();
                }
                list.Add(model);
            }
            mapTemplate.Life = list.ToArray();
        }

        private void ProcessCoconut(MapTemplate mapTemplate, XElement item)
        {
            var model = new MapCoconutTemplate();
            foreach (var prop in item.Elements())
            {
                var propName = prop.GetName();
                if (propName == "effectWin")
                    model.EffectWin = prop.GetStringValue() ?? "";
                else if (propName == "effectLose")
                    model.EffectLose = prop.GetStringValue() ?? "";
                else if (propName == "soundWin")
                    model.SoundWin = prop.GetStringValue() ?? "";
                else if (propName == "soundLose")
                    model.SoundLose = prop.GetStringValue() ?? "";

                else if (propName == "timeDefault")
                    model.TimeDefault = prop.GetIntValue();
                else if (propName == "timeExpand")
                    model.TimeExpand = prop.GetIntValue();
                else if (propName == "timeFinish")
                    model.TimeFinish = prop.GetIntValue();

                else if (propName == "countFalling")
                    model.CountFalling = prop.GetIntValue();
                else if (propName == "countBombing")
                    model.CountBombing = prop.GetIntValue();
                else if (propName == "countStopped")
                    model.CountStopped = prop.GetIntValue();
                else if (propName == "countHit")
                    model.CountHit = prop.GetIntValue();
            }
            mapTemplate.Coconut = model;
        }

        private void ProcessSnowball(MapTemplate mapTemplate, XElement item)
        {
            var template = new MapSnowballTemplate();
            foreach (var prop in item.Elements())
            {
                var propName = prop.GetName();
                if (propName == "damageSnowMan0")
                    template.DamageSnowMan0 = prop.GetIntValue();
                else if (propName == "damageSnowMan1")
                    template.DamageSnowMan1 = prop.GetIntValue();
                else if (propName == "damageSnowBall")
                    template.DamageSnowBall = prop.GetIntValue();
                else if (propName == "recoveryAmount")
                    template.RecoveryAmount = prop.GetIntValue();
                else if (propName == "snowManHP")
                    template.SnowManHP = prop.GetIntValue();
                else if (propName == "snowManWait")
                    template.SnowManWait = prop.GetIntValue();
            }
            mapTemplate.Snowball = template;
        }

        /// <summary>
        /// 加载monsterCarnival节数据
        /// </summary>
        /// <param name="mapTemplate"></param>
        /// <param name="item"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessMC(MapTemplate mapTemplate, XElement node)
        {
            var model = new MapMonsterCarnivalTemplate();
            foreach (var prop in node.Elements())
            {
                var propName = prop.GetName();
                var propValue = prop.Attribute("value")?.Value;
                if (propName == "effectWin")
                    model.EffectWin = prop.GetStringValue();
                else if (propName == "effectLose")
                    model.EffectLose = prop.GetStringValue();
                else if (propName == "soundWin")
                    model.SoundWin = prop.GetStringValue();
                else if (propName == "soundLose")
                    model.SoundLose = prop.GetStringValue();

                else if (propName == "timeDefault")
                    model.TimeDefault = prop.GetIntValue();
                else if (propName == "timeExpand")
                    model.TimeExpand = prop.GetIntValue();
                else if (propName == "timeFinish")
                    model.TimeFinish = prop.GetIntValue();

                else if (propName == "rewardMapWin")
                    model.RewardMapWin = prop.GetIntValue();
                else if (propName == "rewardMapLose")
                    model.RewardMapLose = prop.GetIntValue();

                else if (propName == "reactorRed")
                    model.ReactorRed = prop.GetIntValue();
                else if (propName == "reactorBlue")
                    model.ReactorBlue = prop.GetIntValue();

                else if (propName == "mobGenMax")
                    model.MaxMobs = prop.GetIntValue();
                else if (propName == "guardianGenMax")
                    model.MaxReactors = prop.GetIntValue();
                else if (propName == "deathCP")
                    model.DeathCP = prop.GetIntValue();

                else if (propName == "skill")
                {
                    List<int> list = [];
                    foreach (var skillItem in prop.Elements())
                    {
                        list.Add(skillItem.GetIntValue());
                    }
                    model.Skills = list.ToArray();
                }

                else if (propName == "guardianGenPos")
                {
                    List<MonsterCarnivalGuardianData> list = [];
                    foreach (var dataItem in prop.Elements())
                    {
                        var data = new MonsterCarnivalGuardianData();
                        foreach (var dataProp in dataItem.Elements())
                        {
                            var dataPropName = dataProp.GetName();
                            if (dataPropName == "x")
                                data.X = dataProp.GetIntValue();
                            else if (dataPropName == "y")
                                data.Y = dataProp.GetIntValue();
                            else if (dataPropName == "team")
                                data.Team = dataProp.GetIntValue();
                        }
                        list.Add(data);
                    }
                    model.Guardians = list.ToArray();
                }

                else if (propName == "mob")
                {
                    List<MonsterCarnivalMobData> list = [];
                    foreach (var dataItem in prop.Elements())
                    {
                        var data = new MonsterCarnivalMobData();
                        foreach (var dataProp in dataItem.Elements())
                        {
                            var dataPropName = dataProp.GetName();
                            if (dataPropName == "id")
                                data.Id = dataProp.GetIntValue();
                            else if (dataPropName == "spendCP")
                                data.SpendCP = dataProp.GetIntValue();
                        }
                        list.Add(data);
                    }
                    model.Mobs = list.ToArray();
                }

            }
            mapTemplate.MonsterCarnival = model;
        }
    }
}