using Application.Core.Channel;
using Application.Core.Channel.Commands;

namespace Application.Core.Scripting.Events
{
    public class GuildQuestEventManager : SoloEventManager
    {
        private Queue<int> queuedGuilds = new();
        private Dictionary<int, int> queuedGuildLeaders = new();

        public GuildQuestEventManager(WorldChannel cserv, IEngine iv, ScriptFile file) : base(cserv, iv, file)
        {
        }

        private void exportReadyGuild(int guildId)
        {
            string callout = "[Guild Quest] Your guild has been registered to attend to the Sharenian Guild Quest at channel " + this.getChannelServer().getId()
                + " and HAS JUST STARTED THE STRATEGY PHASE. After 3 minutes, no more guild members will be allowed to join the effort."
                + " Check out Shuang at the excavation site in Perion for more info.";

            cserv.Post(new SendGuildMessageCommand(guildId, 6, callout));
        }

        private void exportMovedQueueToGuild(int guildId, int place)
        {
            string callout = "[Guild Quest] Your guild has been registered to attend to the Sharenian Guild Quest at channel " + this.getChannelServer().getId()
                    + " and is currently on the " + ClientCulture.SystemCulture.Ordinal(place) + " place on the waiting queue.";

            cserv.Post(new SendGuildMessageCommand(guildId, 6, callout));
        }

        private List<int>? getNextGuildQueue()
        {
            if (!queuedGuilds.TryDequeue(out var guildId))
                return null;

            cserv.Node.Transport.RemoveGuildQueued(guildId);
            var leaderId = queuedGuildLeaders.Remove(guildId, out var d) ? d : 0;

            int place = 1;
            foreach (int i in queuedGuilds)
            {
                exportMovedQueueToGuild(i, place);
                place++;
            }

            List<int> list = new(2);
            list.Add(guildId);
            list.Add(leaderId);
            return list;
        }
        public bool isQueueFull()
        {
            return queuedGuilds.Count >= YamlConfig.config.server.EVENT_MAX_GUILD_QUEUE;
        }
        /// <summary>
        /// StartInstance?
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="leaderId"></param>
        /// <returns></returns>
        public sbyte addGuildToQueue(int guildId, int leaderId)
        {
            if (cserv.Node.Transport.IsGuildQueued(guildId))
            {
                return -1;
            }

            if (!isQueueFull())
            {
                bool canStartAhead;

                canStartAhead = queuedGuilds.Count == 0;

                queuedGuilds.Enqueue(guildId);
                cserv.Node.Transport.PutGuildQueued(guildId);
                queuedGuildLeaders.AddOrUpdate(guildId, leaderId);

                int place = queuedGuilds.Count;
                exportMovedQueueToGuild(guildId, place);

                if (canStartAhead)
                {
                    if (!attemptStartGuildInstance())
                    {
                        queuedGuilds.Enqueue(guildId);
                        cserv.Node.Transport.PutGuildQueued(guildId);
                        queuedGuildLeaders.AddOrUpdate(guildId, leaderId);
                    }
                    else
                    {
                        return 2;
                    }
                }

                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool attemptStartGuildInstance()
        {
            Player? chr = null;
            List<int>? guildInstance = null;
            while (chr == null)
            {
                guildInstance = getNextGuildQueue();
                if (guildInstance == null)
                {
                    return false;
                }

                chr = cserv.getPlayerStorage().getCharacterById(guildInstance.get(1));
            }

            if (startInstance(chr))
            {
                exportReadyGuild(guildInstance!.get(0));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
