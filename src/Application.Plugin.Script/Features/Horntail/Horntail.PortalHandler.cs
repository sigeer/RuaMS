using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Horntail
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        /// <summary>
        /// 事件不在场时返回 null，门保持关闭而不是抛异常。
        /// </summary>
        AbstractEventInstanceManager? GetEventInstance()
        {
            var eim = getEventInstance();
            if (eim == null || (eim.EventName != HorntailFeature.ExpeditionEventName && eim.EventName != HorntailFeature.PartyQuestEventName))
            {
                return null;
            }

            return eim;
        }

        // Map: 240050100 | pt: 7
        /// <summary>
        /// 进入封印内部：最后一名玩家必须等待钥匙，持有 1st Crystal Key 时不允许通过。
        /// </summary>
        public async Task<bool> hontale_BtoB1()
        {
            if (getMap().countPlayers() == 1)
            {
                await getPlayer().dropMessage(6, "As the last player on this map, you are compelled to wait for the incoming keys.");
                return false;
            }

            if (haveItem(4001087))
            {
                await getPlayer().dropMessage(6, "You cannot pass to the next map holding the 1st Crystal Key in your inventory.");
                return false;
            }

            await playPortalSound();
            await warp(240050101, 0);
            return true;
        }

        // Map: 240050101, 240050102, 240050103, 240050104, 240050105 | pt: 7
        public async Task<bool> hontale_Bopen()
        {

            int nextMap;
            AbstractEventInstanceManager eim;
            IMap target;
            IPortal? targetPortal;
            string? avail;

            if (getPlayer().getMapId() == 240050101)
            {
                nextMap = 240050102;
                eim = GetEventInstance();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("1stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050102)
            {
                nextMap = 240050103;
                eim = GetEventInstance();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("2stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050103)
            {
                nextMap = 240050104;
                eim = GetEventInstance();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("3stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050104)
            {
                nextMap = 240050105;
                eim = GetEventInstance();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("4stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050105)
            {
                nextMap = 240050100;
                eim = GetEventInstance();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("st00");

                avail = eim.getProperty("5stageclear");
                if (avail == null)
                {
                    if (haveItem(4001092) && isEventLeader())
                    {
                        await eim.showClearEffect();
                        await LightBlue("The leader's key break the seal for a flash...");
                        await playPortalSound();
                        await getPlayer().changeMap(target, targetPortal);
                        eim.setIntProperty("5stageclear", 1);
                        return true;
                    }
                    else
                    {
                        await LightBlue("Horntail\'s Seal is blocking this door. Only the leader with the key can lift this seal.");
                        return false;
                    }
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            return true;
        }

        // Map: 240060000, 240060100 | pt: 8
        public async Task<bool> hontale_BR()
        {

            if (getPlayer().getMapId() == 240060000)
            {
                if (GetEventInstance().getIntProperty("defeatedHead") >= 1)
                {
                    await playPortalSound();
                    await warp(240060100, 0);
                    return true;
                }
                else
                {
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
            }
            else if (getPlayer().getMapId() == 240060100)
            {
                if (GetEventInstance().getIntProperty("defeatedHead") >= 2)
                {
                    await playPortalSound();
                    await warp(240060200, 0);
                    return true;
                }
                else
                {
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
            }
            return false;
        }

        // Map: 240050200 | pt: 7
        public async Task<bool> hontale_C()
        {

            var eim = GetEventInstance();

            if (isEventLeader() == true)
            {
                int target;
                var theWay = getMap().getReactorByName("light")!.getState();
                if (theWay == 1)
                {
                    target = 240050300; //light
                }
                else if (theWay == 3)
                {
                    target = 240050310; //dark
                }
                else
                {
                    await Pink("Hit the Lightbulb to determine your fate!");
                    return false;
                }

                await playPortalSound();
                await eim.warpEventTeam(target);
                return true;
            }
            else
            {
                await Pink("You are not the party leader. Only the party leader may proceed through this portal.");
                return false;
            }
        }

        // Map: 240050600 | pt: 7
        /// <summary>
        /// 远征结束后从巢穴回到集合点。
        /// </summary>
        public async Task<bool> hontale_out1()
        {
            await playPortalSound();
            await warp(240050400, "sp");
            return true;
        }

        // Map: 240040700 | pt: 7
        /// <summary>
        /// 从远征入口传送到 Boss 前的走廊。
        /// </summary>
        public async Task<bool> hontale_morph2()
        {
            await playPortalSound();
            await warp(240040600, 4);
            return true;
        }
    }
}
