using Application.Core.Game.Maps;
using Application.Core.Game.Trades;
using Application.Shared.WzEntity;
using server.maps;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player : IMapPlayer
    {


        public override void setMap(IMap map)
        {
            this.Map = map.getId();
            base.setMap(map);
        }

        public int getMapId()
        {
            if (base.MapModel != null)
            {
                return MapModel.getId();
            }
            return Map;
        }
        public IMap getWarpMap(int map)
        {
            IMap warpMap;
            var eim = getEventInstance();
            if (eim != null)
            {
                warpMap = eim.getMapInstance(map);
            }
            else
            {
                warpMap = Client.getChannelServer().getMapFactory().getMap(map);
            }
            return warpMap;
        }

        private void eventChangedMap(int map)
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.changedMap(this, map);
            }
        }

        public void changeMapBanish(BanishInfo? banishInfo)
        {
            if (banishInfo == null)
                return;

            if (banishInfo.msg != null)
            {
                Pink(banishInfo.msg);
            }

            IMap map_ = getWarpMap(banishInfo.map);
            var portal_ = map_.getPortal(banishInfo.portal);
            changeMap(map_, portal_ != null ? portal_ : map_.getRandomPlayerSpawnpoint());
        }

        public void changeMap(int map)
        {
            IMap warpMap = getWarpMap(map);
            changeMap(warpMap, warpMap.getRandomPlayerSpawnpoint());
        }

        public void changeMap(int map, int portal)
        {
            IMap warpMap = getWarpMap(map);
            changeMap(warpMap, warpMap.getPortal(portal));
        }

        public void changeMap(int map, string portal)
        {
            IMap warpMap = getWarpMap(map);
            changeMap(warpMap, warpMap.getPortal(portal));
        }

        public void changeMap(int map, Portal portal)
        {
            changeMap(getWarpMap(map), portal);
        }

        public void changeMap(IMap to, int portal = 0)
        {
            changeMap(to, to.getPortal(portal));
        }

        public void changeMap(IMap to, Portal? pto)
        {
            eventChangedMap(to.getId());    // player can be dropped from an event here, hence the new warping target.
            pto ??= to.getPortal(0)!;
            changeMapInternal(to, pto.getPosition(), PacketCreator.getWarpToMap(to, pto.getId(), this));
        }

        public void changeMap(IMap to, Point pos)
        {
            eventChangedMap(to.getId());
            changeMapInternal(to, pos, PacketCreator.getWarpToMap(to, 0x80, pos, this));
        }

        private void changeMapInternal(IMap to, Point pos, Packet warpPacket)
        {
            if (mapTransitioning)
                return;

            this.mapTransitioning.Set(true);

            var from = MapModel;
            from.Send(m =>
            {
                this.unregisterChairBuff();
                getTrade()?.CancelTrade(TradeResult.UNSUCCESSFUL_ANOTHER_MAP);
                this.closePlayerInteractions();

                sendPacket(warpPacket);

                m.removePlayer(this);
                if (isLoggedinWorld())
                {
                    setPosition(pos);
                    to.Send(t =>
                    {
                        t.addPlayer(this);
                        Client.CurrentServer.NodeService.BatchSynMapManager.Enqueue(new SyncProto.MapSyncDto { MasterId = Id, MapId = t.getId() });
                    });
                }
                else
                {
                    Log.Warning("Chr {CharacterName} got stuck when moving to map {MapId}", getName(), m.getId());
                    Client.Disconnect(true, false);
                }
            });
        }

        public bool isChangingMaps()
        {
            return this.mapTransitioning.Get();
        }

        public void setMapTransitionComplete()
        {
            this.mapTransitioning.Set(false);
        }


        public void forceChangeMap(IMap target, Portal? pto = null)
        {
            // will actually enter the map given as parameter, regardless of being an eventmap or whatnot

            eventChangedMap(MapId.NONE);

            var mapEim = target.getEventInstance();
            if (mapEim != null)
            {
                var playerEim = this.getEventInstance();
                if (playerEim != null)
                {
                    playerEim.exitPlayer(this);
                    if (playerEim.getPlayerCount() == 0)
                    {
                        playerEim.Dispose();
                    }
                }

                // thanks Thora for finding an issue with players not being actually warped into the target event map (rather sent to the event starting map)
                mapEim.registerPlayer(this, false);
            }

            IMap to = target; // warps directly to the target intead of the target's map id, this allows GMs to patrol players inside instances.
            if (pto == null)
            {
                pto = to.getPortal(0)!;
            }
            changeMapInternal(to, pto.getPosition(), PacketCreator.getWarpToMap(to, pto.getId(), this));
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
        public void startMapEffect(string msg, int itemId, int duration = 30000)
        {
            if (_mapEffect != null)
            {
                return;
            }
            _mapEffect = new MapEffect(msg, itemId, Client.CurrentServer.Node.getCurrentTime() + duration);
            sendPacket(_mapEffect.makeStartData());
        }

        public void showMapOwnershipInfo(Player mapOwner)
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

                this.sendPacket(PacketCreator.getAvatarMega(mapOwner, medal, this.getClient().getChannel(), ItemId.ROARING_TIGER_MESSENGER, strLines, true));
            }
        }
        public void ForcedWarpOut()
        {
            forceChangeMap(MapModel.getForcedReturnMap());
        }
    }
}
