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


using Application.Core.Channel.Commands;
using Application.Core.Game.Maps;
using net.server.services.task.channel;
using server.partyquest;
using tools;

namespace server.maps;


/// <summary>
/// 箱子之类的地图上可攻击的对象？
/// @author Lerk
/// @author Ronan
/// </summary>
public class Reactor : AbstractMapObject
{
    private int rid;
    private ReactorStats stats;
    private sbyte state;
    private sbyte evstate;
    private int delay;

    private string name;
    private bool alive;
    private bool shouldCollect;
    private bool attackHit;
    private ScheduledFuture? timeoutTask = null;
    private IWorldChannelCommand? delayedRespawnRun = null;
    private GuardianSpawnPoint? guardian = null;
    private sbyte facingDirection = 0;

    public Reactor(ReactorStats stats, int rid)
    {
        this.evstate = 0;
        this.stats = stats;
        this.rid = rid;
        this.alive = true;
    }

    public void setShouldCollect(bool collect)
    {
        this.shouldCollect = collect;
    }

    public bool getShouldCollect()
    {
        return shouldCollect;
    }

    public void setState(sbyte state)
    {
        this.state = state;
    }

    public sbyte getState()
    {
        return state;
    }

    public void setEventState(sbyte substate)
    {
        this.evstate = substate;
    }

    public sbyte getEventState()
    {
        return evstate;
    }

    public ReactorStats getStats()
    {
        return stats;
    }

    public int getId()
    {
        return rid;
    }

    public void setDelay(int delay)
    {
        this.delay = delay;
    }

    public int getDelay()
    {
        return delay;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.REACTOR;
    }

    public int getReactorType()
    {
        return stats.getType(state);
    }

    public bool isRecentHitFromAttack()
    {
        return attackHit;
    }

    public ItemQuantity? getReactItem(sbyte index)
    {
        return stats.getReactItem(state, index);
    }

    public bool isAlive()
    {
        return alive;
    }

    public bool isActive()
    {
        return alive && stats.getType(state) != -1;
    }

    public void setAlive(bool alive)
    {
        this.alive = alive;
    }

    public override void sendDestroyData(IChannelClient client)
    {
        client.sendPacket(makeDestroyData());
    }

    public Packet makeDestroyData()
    {
        return PacketCreator.destroyReactor(this);
    }

    public override void sendSpawnData(IChannelClient client)
    {
        if (this.isAlive())
        {
            client.sendPacket(makeSpawnData());
        }
    }

    public Packet makeSpawnData()
    {
        return PacketCreator.spawnReactor(this);
    }

    public void resetReactorActions(int newState)
    {
        setState((sbyte)newState);
        cancelReactorTimeout();
        setShouldCollect(true);
        refreshReactorTimeout();

        if (MapModel != null)
        {
            MapModel.searchItemReactors(this);
        }
    }

    public void forceHitReactor(sbyte newState)
    {
        this.resetReactorActions(newState);
        MapModel.broadcastMessage(PacketCreator.triggerReactor(this, 0));
    }

    public void tryForceHitReactor(sbyte newState)
    {  // weak hit state signal, if already changed reactor state before timeout then drop this

        this.resetReactorActions(newState);
        MapModel.broadcastMessage(PacketCreator.triggerReactor(this, 0));
    }

    public void cancelReactorTimeout()
    {
        if (timeoutTask != null)
        {
            timeoutTask.cancel(false);
            timeoutTask = null;
        }
    }

    private void refreshReactorTimeout()
    {
        int timeOut = stats.getTimeout(state);
        if (timeOut > -1)
        {
            sbyte nextState = stats.getTimeoutState(state);

            timeoutTask = MapModel.ChannelServer.Node.TimerManager.schedule(() =>
            {
                timeoutTask = null;

                MapModel.ChannelServer.Post(new ReactorSetStateCommand(this, nextState));

            }, timeOut);
        }
    }

    public void delayedHitReactor(IChannelClient c, long delay)
    {
        c.CurrentServer.Node.TimerManager.schedule(() =>
        {
            c.CurrentServer.Post(new ReactorHitCommand(this, c));
        }, delay);
    }

    public void hitReactor(IChannelClient c)
    {
        hitReactor(false, 0, 0, 0, c);
    }

    public void hitReactor(bool wHit, int charPos, short stance, int skillid, IChannelClient c)
    {
        try
        {
            if (!this.isActive())
            {
                return;
            }


            cancelReactorTimeout();
            attackHit = wHit;

            if (YamlConfig.config.server.USE_DEBUG)
            {
                c.OnlinedCharacter.dropMessage(5, "Hitted REACTOR " + this.getId() + " with POS " + charPos + " , STANCE " + stance + " , SkillID " + skillid + " , STATE " + state + " STATESIZE " + stats.getStateSize(state));
            }
            c.CurrentServer.ReactorScriptManager.onHit(c, this);

            int reactorType = stats.getType(state);
            if (reactorType < 999 && reactorType != -1)
            {
                //type 2 = only hit from right (kerning swamp plants), 00 is air left 02 is ground left
                if (!(reactorType == 2 && (stance == 0 || stance == 2)))
                {
                    //get next state
                    for (byte b = 0; b < stats.getStateSize(state); b++)
                    {
                        //YAY?
                        var activeSkills = stats.getActiveSkills(state, b);
                        if (activeSkills != null)
                        {
                            if (!activeSkills.Contains(skillid))
                            {
                                continue;
                            }
                        }

                        this.state = stats.getNextState(state, b);
                        sbyte nextState = stats.getNextState(state, b);
                        bool isInEndState = nextState < this.state;
                        if (isInEndState)
                        {
                            //end of reactor
                            if (reactorType < 100)
                            {
                                //reactor broken
                                if (delay > 0)
                                {
                                    MapModel.destroyReactor(getObjectId());
                                }
                                else
                                {
                                    //trigger as normal
                                    MapModel.broadcastMessage(PacketCreator.triggerReactor(this, stance));
                                }
                            }
                            else
                            {
                                //item-triggered on step
                                MapModel.broadcastMessage(PacketCreator.triggerReactor(this, stance));
                            }

                            c.CurrentServer.ReactorScriptManager.act(c, this);
                        }
                        else
                        {
                            //reactor not broken yet
                            MapModel.broadcastMessage(PacketCreator.triggerReactor(this, stance));
                            if (state == stats.getNextState(state, b))
                            {
                                //current state = next state, looping reactor
                                c.CurrentServer.ReactorScriptManager.act(c, this);
                            }

                            setShouldCollect(true);     // refresh collectability on item drop-based reactors
                            refreshReactorTimeout();
                            if (stats.getType(state) == 100)
                            {
                                MapModel.searchItemReactors(this);
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                state++;
                MapModel.broadcastMessage(PacketCreator.triggerReactor(this, stance));
                if (this.getId() != 9980000 && this.getId() != 9980001)
                {
                    c.CurrentServer.ReactorScriptManager.act(c, this);
                }

                setShouldCollect(true);
                refreshReactorTimeout();
                if (stats.getType(state) == 100)
                {
                    MapModel.searchItemReactors(this);
                }
            }


        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    /// <summary>
    /// 只要箱子能重生，就不会从地图上移除，只是看不见
    /// </summary>
    /// <returns></returns>
    public bool destroy()
    {
        bool alive = this.isAlive();
        // reactor neither alive nor in delayed respawn, remove map object allowed
        if (alive)
        {
            this.setAlive(false);
            this.cancelReactorTimeout();

            if (this.getDelay() > 0)
            {
                this.delayedRespawn();
            }
        }
        else
        {
            return delayedRespawnRun == null;
        }


        MapModel.broadcastMessage(PacketCreator.destroyReactor(this));
        return false;
    }

    public void respawn(bool fromDestroyed = true)
    {
        this.resetReactorActions(0);
        this.setAlive(true);

        if (fromDestroyed)
        {
            MapModel.broadcastMessage(this.makeSpawnData());
        }
        else
        {
            MapModel.broadcastMessage(PacketCreator.triggerReactor(this, 0));
        }

    }

    public void delayedRespawn()
    {
        delayedRespawnRun = new ReactorRespawnCommand(this, true);

        OverallService service = MapModel.getChannelServer().OverallService;
        service.registerOverallAction(MapModel.getId(), delayedRespawnRun, this.getDelay());
    }

    public bool forceDelayedRespawn()
    {
        if (delayedRespawnRun != null)
        {
            OverallService service = MapModel.getChannelServer().OverallService;
            service.forceRunOverallAction(MapModel.getId(), delayedRespawnRun);
            return true;
        }
        else
        {
            return false;
        }
    }

    public Rectangle getArea()
    {
        return new Rectangle(getPosition().X + stats.getTL().X, getPosition().Y + stats.getTL().Y, stats.getBR().X - stats.getTL().X, stats.getBR().Y - stats.getTL().Y);
    }

    public string getName()
    {
        return name;
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public GuardianSpawnPoint? getGuardian()
    {
        return guardian;
    }

    public void setGuardian(GuardianSpawnPoint? guardian)
    {
        this.guardian = guardian;
    }

    public void setFacingDirection(sbyte facingDirection)
    {
        this.facingDirection = facingDirection;
    }

    public sbyte getFacingDirection()
    {
        return facingDirection;
    }

    public override int GetSourceId()
    {
        return getId();
    }

    public override string GetName()
    {
        return getName();
    }

    public void HitByMapItem(MapItem mapItem)
    {
        if (getReactorType() == 100)
        {
            if (getShouldCollect() == true && mapItem != null && mapItem == MapModel.getMapObject(mapItem.getObjectId()))
            {
                    if (mapItem.isPickedUp())
                    {
                        return;
                    }

                    var ownerClient = mapItem.getOwnerClient();
                    if (ownerClient == null)
                    {
                        return;
                    }
                    mapItem.setPickedUp(true);
                    MapModel.unregisterItemDrop(mapItem);

                    setShouldCollect(false);
                    MapModel.broadcastMessage(PacketCreator.removeItemFromMap(mapItem.getObjectId(), DropLeaveFieldType.Expired, 0), mapItem.getPosition());

                    hitReactor(ownerClient);

                    if (getDelay() > 0)
                    {
                        var reactorMap = getMap();

                        OverallService service = reactorMap.getChannelServer().OverallService;
                        service.registerOverallAction(reactorMap.getId(), new ReactorRespawnCommand(this, false), getDelay());
                    }
  
            }
        }
    }
}
