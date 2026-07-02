using Application.Core.Game.Maps;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Game.Trades;
using Application.Shared.WzEntity;
using server.maps;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public override async Task OnMounted(IMap map)
        {
            await base.OnMounted(map);
            this.Map = map.getId();

            var pets = getPets();
            foreach (var pet in pets)
            {
                if (pet != null)
                {
                    await MapModel.AddMapObject(pet, null);
                    await pet.sendSpawnData(Client);

                    // 宠物首次进入地图是通过 spawnPlayerMapObject 此时必然会对自己显示
                    await MapModel.SetPlayerVisibleObject(this, pet, false);
                }
            }
            await commitExcludedItems();
        }

        public override async Task OnUnmounted()
        {
            await unregisterChairBuff();

            await releaseControlledMonsters();
            setChair(-1);

            foreach (Summon summon in getSummonsValues())
            {
                if (summon.isStationary())
                {
                    await cancelEffectFromBuffStat(BuffStat.PUPPET);
                }
                else
                {
                    await MapModel.RemoveMapObject(summon, p => summon.sendDestroyData(p.Client));
                }
            }

            var dragon = getDragon();
            if (dragon != null)
            {
                await MapModel.RemoveMapObject(dragon, p => dragon.sendDestroyData(p.Client));
            }

            foreach (var pet in getPets())
            {
                if (pet != null)
                {
                    // 似乎不需要另外再销毁
                    await MapModel.RemoveMapObject(pet, null);
                }

            }
        }
        public int getMapId()
        {
            if (base.MapModel != null)
            {
                return MapModel.getId();
            }
            return Map;
        }
        public async Task<IMap> getWarpMap(int map)
        {
            IMap warpMap;
            var eim = getEventInstance();
            if (eim != null)
            {
                warpMap = await eim.getMapInstance(map);
            }
            else
            {
                warpMap = await Client.getChannelServer().getMapFactory().getMap(map);
            }
            return warpMap;
        }

        private async Task eventChangedMap(int map)
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                await eim.changedMap(this, map);
            }
        }

        public async Task changeMapBanish(BanishInfo? banishInfo)
        {
            if (banishInfo == null)
                return;

            if (banishInfo.msg != null)
            {
                await Pink(banishInfo.msg);
            }

            IMap map_ = await getWarpMap(banishInfo.map);
            var portal_ = map_.getPortal(banishInfo.portal);
            await changeMap(map_, portal_ != null ? portal_ : map_.getRandomPlayerSpawnpoint());
        }

        public async Task changeMap(int map)
        {
            IMap warpMap = await getWarpMap(map);
            await changeMap(warpMap, warpMap.getRandomPlayerSpawnpoint());
        }

        public async Task changeMap(int map, int portal)
        {
            IMap warpMap = await getWarpMap(map);
            await changeMap(warpMap, warpMap.getPortal(portal));
        }

        public async Task changeMap(int map, string portal)
        {
            IMap warpMap = await getWarpMap(map);
            await changeMap(warpMap, warpMap.getPortal(portal));
        }

        public async Task changeMap(int map, Portal? portal)
        {
            await changeMap(await getWarpMap(map), portal);
        }

        public async Task changeMap(IMap to, int portal = 0)
        {
            await changeMap(to, to.getPortal(portal));
        }

        public async Task changeMap(IMap to, Portal? pto)
        {
            await eventChangedMap(to.getId());
            pto ??= to.getPortal(0)!;
            var warpTo = await getWarpMap(to.getId());
            await changeMapInternal(warpTo, pto.getPosition(), PacketCreator.getWarpToMap(warpTo, pto.getId(), this));
        }

        public async Task changeMap(IMap to, Point pos)
        {
            await eventChangedMap(to.getId());
            var warpTo = await getWarpMap(to.getId());
            await changeMapInternal(warpTo, pos, PacketCreator.getWarpToMap(warpTo, 0x80, pos, this));
        }

        private async Task changeMapInternal(IMap to, Point pos, Packet warpPacket)
        {
            if (mapTransitioning)
                return;

            this.mapTransitioning.Set(true);

            await this.unregisterChairBuff();
            await closeTrade(TradeResult.UNSUCCESSFUL_ANOTHER_MAP);
            await this.closePlayerInteractions();

            await MapModel.removePlayer(this);
            if (isLoggedinWorld())
            {
                await to.Send(async t =>
                {
                    setPosition(pos);

                    await SendPacket(warpPacket);
                    await t.addPlayer(this);

                    Client.CurrentServer.NodeService.BatchSynMapManager.Enqueue(new SyncProto.MapSyncDto { MasterId = Id, MapId = t.getId() });
                });
            }
            else
            {
                Log.Warning("Chr {CharacterName} got stuck when moving to map {MapId}", getName(), to.getId());
                await Client.Disconnect(true, false);
            }
        }

        public bool isChangingMaps()
        {
            return this.mapTransitioning.Get();
        }

        public void setMapTransitionComplete()
        {
            this.mapTransitioning.Set(false);
        }


        public async Task forceChangeMap(IMap target, Portal? pto = null)
        {
            // will actually enter the map given as parameter, regardless of being an eventmap or whatnot

            await eventChangedMap(MapId.NONE);

            var mapEim = target.getEventInstance();
            if (mapEim != null)
            {
                var playerEim = this.getEventInstance();
                if (playerEim != null)
                {
                    await playerEim.exitPlayer(this);
                }

                // thanks Thora for finding an issue with players not being actually warped into the target event map (rather sent to the event starting map)
                await mapEim.registerPlayer(this);
            }

            IMap to = target; // warps directly to the target intead of the target's map id, this allows GMs to patrol players inside instances.
            if (pto == null)
            {
                pto = to.getPortal(0)!;
            }
            await changeMapInternal(to, pto.getPosition(), PacketCreator.getWarpToMap(to, pto.getId(), this));
        }

        private int getVisitedMapIndex(IMap map)
        {
            int idx = 0;

            foreach (WeakReference<IMap> mapRef in lastVisitedMaps)
            {
                if (mapRef.TryGetTarget(out var d) && map == d)
                    return idx;

                idx++;
            }

            return -1;
        }

        private HashSet<int> entered = new();
        public void enteredScript(int mapid)
        {
            entered.Add(mapid);
        }

        public void resetEnteredScript()
        {
            entered.Remove(MapModel.getId());
        }

        public void resetEnteredScript(int mapId)
        {
            entered.Remove(mapId);
        }

        public bool hasEntered(int mapId)
        {
            return entered.Contains(mapId);
        }


        public void visitMap(IMap map)
        {
            int idx = getVisitedMapIndex(map);

            if (idx == -1)
            {
                if (lastVisitedMaps.Count == YamlConfig.config.server.MAP_VISITED_SIZE)
                {
                    lastVisitedMaps.RemoveAt(0);
                }
            }
            else
            {
                var mapRef = lastVisitedMaps.remove(idx);
                lastVisitedMaps.Add(mapRef);
                return;
            }

            lastVisitedMaps.Add(new(map));
        }

        public void setOwnedMap(IMap? map)
        {
            ownedMap = new(map);
        }

        public IMap? getOwnedMap()
        {
            if (ownedMap.TryGetTarget(out var d))
            {
                return d;

            }
            return null;
        }

        MapEffect? _mapEffect;
        public async Task startMapEffect(string msg, int itemId, int duration = 30000)
        {
            if (_mapEffect != null)
            {
                return;
            }
            _mapEffect = new MapEffect(msg, itemId, Client.CurrentServer.Node.getCurrentTime() + duration);
            await SendPacket(_mapEffect.makeStartData());
        }

        public async Task showMapOwnershipInfo(Player mapOwner)
        {
            long curTime = Client.CurrentServer.Node.getCurrentTime();
            if (nextWarningTime < curTime)
            {
                nextWarningTime = (long)(curTime + TimeSpan.FromMinutes(1).TotalMilliseconds); // show underlevel info again after 1 minute

                string medal = mapOwner.getMedalText();

                List<string> strLines = new();
                strLines.Add("");
                strLines.Add("");
                strLines.Add("");
                strLines.Insert(this.getClient().getChannelServer().getServerMessage().Count() == 0 ? 0 : 1, "Get off my lawn!!");

                await this.SendPacket(PacketCreator.getAvatarMega(mapOwner, medal, this.getClient().getChannel(), ItemId.ROARING_TIGER_MESSENGER, strLines, true));
            }
        }
        public async Task ForcedWarpOut()
        {
            await changeMap(await MapModel.getForcedReturnMap());
        }
    }
}
