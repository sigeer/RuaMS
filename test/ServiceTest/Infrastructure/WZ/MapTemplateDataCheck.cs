using System.Text;
using Application.Shared.Constants.Map;
using Application.Templates;
using Application.Templates.Map;
using Application.Templates.Reader;
using Newtonsoft.Json;
using XmlWzReader;
using XmlWzReader.wz;

namespace ServiceTest.Infrastructure.WZ;

internal class MapTemplateDataCheck(string readerType) : WzTestBase(readerType)
{
    private static string GetMapImg(int mapid)
    {
        var mapName = mapid.ToString().PadLeft(9, '0');
        var builder = new StringBuilder("Map/Map");
        var area = mapid / 100000000;
        builder.Append(area);
        builder.Append('/');
        builder.Append(mapName);
        builder.Append(".img");
        mapName = builder.ToString();
        return mapName;
    }

    [Test]
    public void MapProvider_LoadAll_ReturnsNonEmpty()
    {
        var provider = _providerSource.GetProvider(ProviderType.Map);
        var maps = provider.LoadAll().ToList();
        Assert.That(maps, Is.Not.Empty);
    }

    [Test]
    public void LoadAll_AllHaveTemplateId()
    {
        var provider = _providerSource.GetProvider(ProviderType.Map);
        var all = provider.LoadAll().OfType<MapTemplate>().ToList();
        var invalid = all.Where(m => m.TemplateId < 0).ToList();
        Assert.That(invalid, Is.Empty, () => $"Maps with TemplateId < 0: {string.Join(",", invalid.Select(m => m.TemplateId))}");
    }

    [Test]
    public void NonExistentMap_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(999999999);
        Assert.That(map, Is.Null);
    }

    [Test]
    public void MapWithoutCoconut_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;
        Assert.That(map.Coconut, Is.Null);
    }

    [Test]
    public void MapWithoutSnowball_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;
        Assert.That(map.Snowball, Is.Null);
    }

    [Test]
    public void MapWithoutMonsterCarnival_ReturnsNull()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;
        Assert.That(map.MonsterCarnival, Is.Null);
    }

    [Test]
    public void GetItem_Henesys_LoadsCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        Assert.That(map.TemplateId, Is.EqualTo(100000000));
        Assert.That(map.ForcedReturn, Is.EqualTo(WzDefaults.MapNone));
        Assert.That(map.Town, Is.True);
        Assert.That(map.FieldType, Is.EqualTo(0));
        Assert.That(map.FixedMobCapacity, Is.EqualTo(500));
        Assert.That(map.MobRate, Is.EqualTo(1f));
        Assert.That(map.RecoveryRate, Is.EqualTo(1f));
        Assert.That(map.Portals, Is.Not.Empty);
        Assert.That(map.Footholds, Is.Not.Empty);
        Assert.That(map.Life, Is.Not.Empty);
        Assert.That(map.HasClock, Is.False);
        Assert.That(map.HasShip, Is.False);
        Assert.That(map.OnUserEnter, Is.EqualTo("explorationPoint"));
        Assert.That(map.Town, Is.True);
        Assert.That(map.ReturnMap, Is.EqualTo(100000000));
        Assert.That(map.ForcedReturn, Is.EqualTo(WzDefaults.MapNone));
        Assert.That(map.FlyMap, Is.False);
        Assert.That(map.FieldLimit, Is.EqualTo(0));
        Assert.That(map.TimeLimit, Is.EqualTo(-1));
        Assert.That(map.FixedMobCapacity, Is.EqualTo(500));
        Assert.That(map.CreateMobInterval, Is.EqualTo(5000));
        Assert.That(map.Everlast, Is.False);
        Assert.That(map.ReactorShuffle, Is.False);
    }

    [Test]
    public void GetItem_Henesys_HasMiniMap()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        Assert.That(map.MiniMap, Is.Not.Null);
        Assert.That(map.MiniMap.Width, Is.EqualTo(7444));
        Assert.That(map.MiniMap.Height, Is.EqualTo(1391));
        Assert.That(map.MiniMap.CenterX, Is.EqualTo(1068));
        Assert.That(map.MiniMap.CenterY, Is.EqualTo(661));
    }

    [Test]
    public void GetItem_Henesys_HasLadderRopes()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        Assert.That(map.LadderRopes, Is.Not.Empty);
    }

    [Test]
    public void GetItem_Henesys_HasNoClockOrShip()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        Assert.That(map.HasClock, Is.False);
        Assert.That(map.HasShip, Is.False);
    }

    [Test]
    public void Henesys_SpecialMarkers_WorkCorrectly()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        var spPortal = map.Portals.First(p => p.sPortalName == "sp");
        Assert.That(spPortal.nIndex, Is.EqualTo(0));

        var life0 = map.Life.First(l => l.X == 3199 && l.Y == 176);
        Assert.That(life0.Index, Is.EqualTo(0));

        var foothold1 = map.Footholds.First(f => f.X1 == 4417 && f.Y1 == 244);
        Assert.That(foothold1.Index, Is.EqualTo(1));
    }

    [Test]
    public void Henesys_HasLadderRopes_WithCorrectValues()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        Assert.That(map.LadderRopes, Is.Not.Empty);

        var ladder1 = map.LadderRopes[0];
        Assert.That(ladder1.X, Is.EqualTo(5370));
        Assert.That(ladder1.Y1, Is.EqualTo(156));
        Assert.That(ladder1.Y2, Is.EqualTo(399));
        Assert.That(ladder1.Type, Is.EqualTo(1));

        var ladder2 = map.LadderRopes[1];
        Assert.That(ladder2.X, Is.EqualTo(3398));
        Assert.That(ladder2.Y1, Is.EqualTo(126));
        Assert.That(ladder2.Y2, Is.EqualTo(332));
        Assert.That(ladder2.Type, Is.EqualTo(1));
    }

    [Test]
    public void Henesys_HasPortal_WithCorrectValues()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(100000000)!;

        var portal0 = map.Portals[0];
        Assert.That(portal0.sPortalName, Is.EqualTo("sp"));
        Assert.That(portal0.nPortalType, Is.EqualTo(0));
        Assert.That(portal0.nX, Is.EqualTo(112));
        Assert.That(portal0.nY, Is.EqualTo(197));
        Assert.That(portal0.nTargetMap, Is.EqualTo(999999999));
        Assert.That(portal0.sTargetName, Is.EqualTo(""));
    }

    [Test]
    public void GetItem_CPQMap_HasMonsterCarnivalData()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(980000101)!;

        Assert.That(map.MonsterCarnival, Is.Not.Null);
        Assert.That(map.MonsterCarnival.TimeDefault, Is.EqualTo(610));
        Assert.That(map.MonsterCarnival.TimeExpand, Is.EqualTo(120));
        Assert.That(map.MonsterCarnival.TimeFinish, Is.EqualTo(12));
        Assert.That(map.MonsterCarnival.EffectWin, Is.EqualTo("quest/carnival/win"));
        Assert.That(map.MonsterCarnival.EffectLose, Is.EqualTo("quest/carnival/lose"));
        Assert.That(map.MonsterCarnival.SoundWin, Is.EqualTo("MobCarnival/Win"));
        Assert.That(map.MonsterCarnival.SoundLose, Is.EqualTo("MobCarnival/Lose"));
        Assert.That(map.MonsterCarnival.RewardMapWin, Is.EqualTo(980000103));
        Assert.That(map.MonsterCarnival.Skills, Is.Not.Empty);
        Assert.That(map.MonsterCarnival.Skills, Does.Contain(1));
        Assert.That(map.MonsterCarnival.Skills, Does.Contain(7));
        Assert.That(map.MonsterCarnival.Mobs, Is.Not.Empty);
        Assert.That(map.MonsterCarnival.Mobs, Has.Some.Matches<MonsterCarnivalMobData>(p => p.Id == 9300128 && p.SpendCP == 7));
        Assert.That(map.MonsterCarnival.Guardians, Is.Not.Empty);
        Assert.That(map.MonsterCarnival.Guardians, Has.Some.Matches<MonsterCarnivalGuardianData>(p => p.X == -538 && p.Y == -135));
    }

    [Test]
    public void Map109080000_HasCoconutData()
    {
        var provider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var map = provider.GetItem(109080000)!;

        Assert.That(map.Coconut, Is.Not.Null);
        Assert.That(map.Coconut.EffectWin, Is.EqualTo("event/coconut/victory"));
        Assert.That(map.Coconut.EffectLose, Is.EqualTo("event/coconut/lose"));
        Assert.That(map.Coconut.SoundWin, Is.EqualTo("Coconut/Victory"));
        Assert.That(map.Coconut.SoundLose, Is.EqualTo("Coconut/Failed"));
        Assert.That(map.Coconut.TimeDefault, Is.EqualTo(300));
        Assert.That(map.Coconut.TimeExpand, Is.EqualTo(120));
        Assert.That(map.Coconut.TimeFinish, Is.EqualTo(12));
        Assert.That(map.Coconut.CountFalling, Is.EqualTo(401));
        Assert.That(map.Coconut.CountBombing, Is.EqualTo(80));
        Assert.That(map.Coconut.CountStopped, Is.EqualTo(20));
        Assert.That(map.Coconut.CountHit, Is.EqualTo(5));
    }

    [Test]
    public void MapEqualCheck()
    {
        var oldProvider = DataProviderFactory.getDataProvider(WZFiles.MAP);
        var oldData = oldProvider.getData(GetMapImg(001000000));
        var infoData = oldData.getChildByPath("info")!;
        var newProvider = _providerSource.GetProvider<IProvider<MapTemplate>>(ProviderType.Map);
        var newData = newProvider.GetItem(001000000)!;

        Assert.That(newData.HasClock, Is.EqualTo(oldData.getChildByPath("clock") != null));
        Assert.That(newData.OnUserEnter, Is.EqualTo(DataTool.getString(infoData?.getChildByPath("onUserEnter"))));
        Assert.That(newData.Everlast, Is.EqualTo(DataTool.getIntConvert("everlast", infoData, 0) != 0));
        Assert.That(newData.Town, Is.EqualTo(DataTool.GetBoolean("town", infoData)));
        Assert.That(newData.DecHP, Is.EqualTo(DataTool.getIntConvert("decHP", infoData, 0)));
        Assert.That(newData.ProtectItem, Is.EqualTo(DataTool.getIntConvert("protectItem", infoData, 0)));
        Assert.That(newData.ForcedReturn, Is.EqualTo(DataTool.getInt(infoData?.getChildByPath("forcedReturn"), MapId.NONE)));
        Assert.That(newData.TimeLimit, Is.EqualTo(DataTool.getIntConvert("timeLimit", infoData, -1)));
        Assert.That(newData.FieldType, Is.EqualTo(DataTool.getIntConvert("fieldType", infoData, 0)));
        Assert.That(newData.FixedMobCapacity, Is.EqualTo(DataTool.getIntConvert("fixedMobCapacity", infoData, 500)));
        Assert.That(newData.Reactors, Has.Some.Matches<MapReactorTemplate>(x => x.Id == 2001));
    }

    [Test]
    public void MapObstacleProvider_LoadsDataWhenAvailable()
    {
        var provider = _providerSource.GetProvider(ProviderType.MapObstacle);
        var all = provider.LoadAll().OfType<MapObstacleTemplate>().ToList();
        if (all.Count > 0)
        {
            Assert.That(all.Any(o => o.MobDamage > 0), Is.True);
        }
    }
}
