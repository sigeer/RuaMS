using Application.Core.Game.Players;
using Application.Core.Scripting.Events;
using scripting.Event;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Application.Plugin.Script.Events
{
    internal class PQ_KerningInstance: AbstractEventInstanceManager
    {
        PQ_KerningEventManager _em;
        public long StartTime { get; }
        public long EndTime { get; }

        public PQ_KerningInstance(PQ_KerningEventManager em, string name) 
        {
            _em = em;

            StartTime = _em.ChannelServer.Node.getCurrentTime();

        }

        protected virtual void End()
        {
            var party = getPlayers();
            foreach (var player in getPlayers())
            {
                unregisterPlayer(player);
                player.changeMap(_em.ExitMap, 0);
            }
            Dispose();
        }

        protected void Respawn()
        {

        }


        public override bool revivePlayer(Player player)
        {
            if (isEventTeamLackingNow(true, _em.MinCount, player))
            {
                unregisterPlayer(player);
                End();
            }
            else
            {
                unregisterPlayer(player);
            }
            return true;

        }
    }
}
