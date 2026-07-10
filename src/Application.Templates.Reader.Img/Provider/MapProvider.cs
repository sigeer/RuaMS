using Application.Templates.Exceptions;
using Application.Templates.Map;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class MapProvider : AbstractGroupProvider<MapTemplate>
    {
        public override ProviderType Type => ProviderType.Map;

        public MapProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<MapTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                var fullPath = _resolver.ResolveFullPath(imgPath);
                var rootNode = new WZImage(fullPath);

                if (!int.TryParse(rootNode.Name.AsSpan(0, 9), out var mapId))
                    throw new TemplateFormatException("Map.wz", imgPath);

                var mapTemplate = new MapTemplate(mapId);

                foreach (var item in rootNode.Children)
                {
                    var nodeType = item.Name;
                    if (nodeType == "info")
                    {
                        foreach (var infoPropNode in item.Children)
                        {
                            var name = infoPropNode.Name;
                            if (name == "link")
                                GetItem(infoPropNode.GetIntValue())?.CloneLink(mapTemplate);
                            else if (name == "forcedReturn")
                                mapTemplate.ForcedReturn = infoPropNode.GetIntValue(defaultValue: WzDefaults.MapNone);
                            else if (name == "returnMap")
                                mapTemplate.ReturnMap = infoPropNode.GetIntValue(defaultValue: WzDefaults.MapNone);
                            else if (name == "fly")
                                mapTemplate.FlyMap = infoPropNode.ResolveBool() ?? false;
                            else if (name == "town")
                                mapTemplate.Town = infoPropNode.ResolveBool() ?? false;
                            else if (name == "everlast")
                                mapTemplate.Everlast = infoPropNode.ResolveBool() ?? false;
                            else if (name == "createMobInterval")
                                mapTemplate.CreateMobInterval = infoPropNode.GetIntValue(defaultValue: 5000);
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
                                mapTemplate.TimeLimit = infoPropNode.GetIntValue(defaultValue: -1);
                            else if (name == "fieldLimit")
                                mapTemplate.FieldLimit = infoPropNode.GetIntValue();
                            else if (name == "fieldType")
                                mapTemplate.FieldType = infoPropNode.GetIntValue();
                            else if (name == "recovery")
                                mapTemplate.RecoveryRate = infoPropNode.GetFloatValue(defaultValue: 1);
                            else if (name == "fixedMobCapacity")
                                mapTemplate.FixedMobCapacity = infoPropNode.GetIntValue(defaultValue: 500);
                            else if (name == "timeMob")
                            {
                                var timeMob = new MapTimeMobTemplate();
                                foreach (var timeMobProp in infoPropNode.Children)
                                {
                                    var timeMobPropName = timeMobProp.Name;
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
                    else if (nodeType == "foothold")
                        ProcessMapFoothold(mapTemplate, item);
                    else if (nodeType == "back")
                        ProcessMapBack(mapTemplate, item);
                    else if (nodeType == "portal")
                        ProcessMapPortal(mapTemplate, item);
                    else if (nodeType == "life")
                        ProcessMapLife(mapTemplate, item);
                    else if (nodeType == "reactor")
                        ProcessMapReactor(mapTemplate, item);
                    else if (nodeType == "ladderRope")
                        ProcessLadderRope(mapTemplate, item);
                    else if (nodeType == "miniMap")
                        ProcessMiniMap(mapTemplate, item);
                    else if (nodeType == "area")
                        ProcessArea(mapTemplate, item);
                    else if (nodeType == "seat")
                        mapTemplate.SeatCount = item.Children.Count();
                    else if (nodeType == "monsterCarnival")
                        ProcessMC(mapTemplate, item);
                    else if (nodeType == "snowball")
                        ProcessSnowball(mapTemplate, item);
                    else if (nodeType == "coconut")
                        ProcessCoconut(mapTemplate, item);
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

        private static void ProcessMiniMap(MapTemplate mapTemplate, IDataNode item)
        {
            var mini = new MapMiniMapTemplate();
            foreach (var prop in item.Children)
            {
                var name = prop.Name;
                if (name == "width") mini.Width = prop.GetIntValue();
                else if (name == "height") mini.Height = prop.GetIntValue();
                else if (name == "centerX") mini.CenterX = prop.GetIntValue();
                else if (name == "centerY") mini.CenterY = prop.GetIntValue();
            }
            mapTemplate.MiniMap = mini;
        }

        private static void ProcessArea(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapAreaTemplate>();
            foreach (var areaItem in item.Children)
            {
                var model = new MapAreaTemplate();
                foreach (var areaProp in areaItem.Children)
                {
                    var name = areaProp.Name;
                    if (name == "x1") model.X1 = areaProp.GetIntValue();
                    else if (name == "x2") model.X2 = areaProp.GetIntValue();
                    else if (name == "y1") model.Y1 = areaProp.GetIntValue();
                    else if (name == "y2") model.Y2 = areaProp.GetIntValue();
                }
                list.Add(model);
            }
            mapTemplate.Areas = [.. list];
        }

        private static void ProcessMapBack(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapBackTemplate>();
            foreach (var backItem in item.Children)
            {
                var model = new MapBackTemplate { Index = int.TryParse(backItem.Name, out var idx) ? idx : 0 };
                foreach (var backProp in backItem.Children)
                {
                    if (backProp.Name == "type")
                    {
                        model.Type = backProp.GetIntValue();
                        break;
                    }
                }
                list.Add(model);
            }
            mapTemplate.Backs = [.. list];
        }

        private static void ProcessLadderRope(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapLadderRopeTemplate>();
            foreach (var nodeItem in item.Children)
            {
                if (!int.TryParse(nodeItem.Name, out var idx)) continue;
                var model = new MapLadderRopeTemplate(idx);
                foreach (var prop in nodeItem.Children)
                {
                    var name = prop.Name;
                    if (name == "x") model.X = prop.GetIntValue();
                    else if (name == "y1") model.Y1 = prop.GetIntValue();
                    else if (name == "y2") model.Y2 = prop.GetIntValue();
                    else if (name == "l") model.Type = prop.GetIntValue();
                }
                list.Add(model);
            }
            mapTemplate.LadderRopes = [.. list];
        }

        private static void ProcessMapFoothold(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapFootholdTemplate>();
            foreach (var fhList in item.Children)
            {
                foreach (var fhList2 in fhList.Children)
                {
                    foreach (var fhItemNode in fhList2.Children)
                    {
                        if (!int.TryParse(fhItemNode.Name, out var fhIdx)) continue;
                        var fhItem = new MapFootholdTemplate { Index = fhIdx };
                        foreach (var fhProp in fhItemNode.Children)
                        {
                            var name = fhProp.Name;
                            if (name == "next") fhItem.Next = fhProp.GetIntValue();
                            else if (name == "prev") fhItem.Prev = fhProp.GetIntValue();
                            else if (name == "x1") fhItem.X1 = fhProp.GetIntValue();
                            else if (name == "x2") fhItem.X2 = fhProp.GetIntValue();
                            else if (name == "y1") fhItem.Y1 = fhProp.GetIntValue();
                            else if (name == "y2") fhItem.Y2 = fhProp.GetIntValue();
                        }
                        list.Add(fhItem);
                    }
                }
            }
            mapTemplate.Footholds = [.. list];
        }

        private static void ProcessMapPortal(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapPortalTemplate>();
            foreach (var portalItem in item.Children)
            {
                if (!int.TryParse(portalItem.Name, out var portalIdx)) continue;
                var model = new MapPortalTemplate { nIndex = portalIdx };
                foreach (var portalProp in portalItem.Children)
                {
                    var name = portalProp.Name;
                    if (name == "pn") model.sPortalName = portalProp.GetStringValue();
                    else if (name == "tn") model.sTargetName = portalProp.GetStringValue();
                    else if (name == "script") model.Script = portalProp.GetStringValue();
                    else if (name == "pt") model.nPortalType = portalProp.GetIntValue();
                    else if (name == "tm") model.nTargetMap = portalProp.GetIntValue();
                    else if (name == "x") model.nX = portalProp.GetIntValue();
                    else if (name == "y") model.nY = portalProp.GetIntValue();
                }
                list.Add(model);
            }
            mapTemplate.Portals = [.. list];
        }

        private static void ProcessMapReactor(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapReactorTemplate>();
            foreach (var reactorItem in item.Children)
            {
                if (!int.TryParse(reactorItem.Name, out var reactorIdx)) continue;
                var model = new MapReactorTemplate
                {
                    Index = reactorIdx,
                    Id = int.TryParse(reactorItem.GetStringValue("id"), out var id) ? id : 0,
                    Name = reactorItem.GetStringValue("name"),
                    F = reactorItem.GetIntValue("f"),
                    ReactorTime = reactorItem.GetIntValue("reactorTime"),
                    X = reactorItem.GetIntValue("x"),
                    Y = reactorItem.GetIntValue("y"),
                };
                list.Add(model);
            }
            mapTemplate.Reactors = list.ToArray();
        }

        private static void ProcessMapLife(MapTemplate mapTemplate, IDataNode item)
        {
            var list = new List<MapLifeTemplate>();
            foreach (var lifeItem in item.Children)
            {
                if (!int.TryParse(lifeItem.Name, out var lifeIdx)) continue;
                var model = new MapLifeTemplate { Index = lifeIdx };
                foreach (var lifeProp in lifeItem.Children)
                {
                    var name = lifeProp.Name;
                    if (name == "id") model.Id = lifeProp.GetIntValue();
                    else if (name == "cy") model.CY = lifeProp.GetIntValue();
                    else if (name == "hide") model.Hide = lifeProp.ResolveBool() ?? false;
                    else if (name == "f") model.F = lifeProp.GetIntValue();
                    else if (name == "fh") model.Foothold = lifeProp.GetIntValue();
                    else if (name == "mobTime") model.MobTime = lifeProp.GetIntValue();
                    else if (name == "type") model.Type = lifeProp.GetStringValue() ?? "";
                    else if (name == "rx0") model.RX0 = lifeProp.GetIntValue();
                    else if (name == "rx1") model.RX1 = lifeProp.GetIntValue();
                    else if (name == "x") model.X = lifeProp.GetIntValue();
                    else if (name == "y") model.Y = lifeProp.GetIntValue();
                    else if (name == "team") model.Team = lifeProp.GetIntValue(defaultValue: -1);
                }
                list.Add(model);
            }
            mapTemplate.Life = [.. list];
        }

        private void ProcessCoconut(MapTemplate mapTemplate, IDataNode item)
        {
            var model = new MapCoconutTemplate();
            foreach (var prop in item.Children)
            {
                var propName = prop.Name;
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

        private void ProcessSnowball(MapTemplate mapTemplate, IDataNode item)
        {
            var template = new MapSnowballTemplate();
            foreach (var prop in item.Children)
            {
                var propName = prop.Name;
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

        private void ProcessMC(MapTemplate mapTemplate, IDataNode node)
        {
            var model = new MapMonsterCarnivalTemplate();
            foreach (var prop in node.Children)
            {
                var propName = prop.Name;
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
                    foreach (var skillItem in prop.Children)
                        list.Add(skillItem.GetIntValue());
                    model.Skills = [.. list];
                }
                else if (propName == "guardianGenPos")
                {
                    List<MonsterCarnivalGuardianData> list = [];
                    foreach (var dataItem in prop.Children)
                    {
                        var data = new MonsterCarnivalGuardianData();
                        foreach (var dataProp in dataItem.Children)
                        {
                            var dataPropName = dataProp.Name;
                            if (dataPropName == "x") data.X = dataProp.GetIntValue();
                            else if (dataPropName == "y") data.Y = dataProp.GetIntValue();
                            else if (dataPropName == "team") data.Team = dataProp.GetIntValue();
                        }
                        list.Add(data);
                    }
                    model.Guardians = [.. list];
                }
                else if (propName == "mob")
                {
                    List<MonsterCarnivalMobData> list = [];
                    foreach (var dataItem in prop.Children)
                    {
                        var data = new MonsterCarnivalMobData();
                        foreach (var dataProp in dataItem.Children)
                        {
                            var dataPropName = dataProp.Name;
                            if (dataPropName == "id") data.Id = dataProp.GetIntValue();
                            else if (dataPropName == "spendCP") data.SpendCP = dataProp.GetIntValue();
                        }
                        list.Add(data);
                    }
                    model.Mobs = [.. list];
                }
            }
            mapTemplate.MonsterCarnival = model;
        }
    }
}
