using Application.Core.scripting.npc;
using Application.Resources.Messages;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using System.Text;

namespace Application.Core.Game.Commands.Gm2
{
    internal class WarpNpcCommand : ParamsCommandBase
    {
        public WarpNpcCommand() : base(["<npc>"], 2, "warpnpc")
        {
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var input = GetParam("npc");
            if (!int.TryParse(input, out var npcId))
            {
                var searched = client.CurrentCulture.StringProvider.Search(Templates.String.StringCategory.Npc, input).OfType<StringNpcTemplate>().ToArray();
                if (searched.Length == 0)
                {
                    client.OnlinedCharacter.Yellow(nameof(ClientMessage.NpcNotFound), input);
                    return;
                }
                else if (searched.Length > 1)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < searched.Length; i++)
                    {
                        var npcItem = searched[i];
                        sb.Append($"\r\n#L{i}# {npcItem.TemplateId} - {npcItem.Name} #l");
                    }
                    TempConversation.Create(client)?.RegisterSelect(sb.ToString(), (i, ctx) =>
                    {
                        HandleNpcId(client, searched[i].TemplateId, searched[i].Name, ctx);
                    });
                }
                else
                {
                    HandleNpcId(client, searched[0].TemplateId, searched[0].Name);
                }
            }
            else
            {
                HandleNpcId(client, npcId, client.CurrentCulture.GetNpcName(npcId));
            }

        }

        void HandleNpcId(IChannelClient client, int npcId, string npcName, TempConversation? conversation = null)
        {
            var template = ProviderFactory.GetProvider<EtcNpcLocationProvider>().GetItem(npcId);
            if (template == null)
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.NpcNotFound), npcName);
                return;
            }

            if (template.Maps.Length == 0)
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.NpcOutOfMap), npcName);
                return;
            }
            else if (template.Maps.Length > 1)
            {
                var sb = new StringBuilder();
                sb.Append(client.CurrentCulture.GetMessageByKey(nameof(ClientMessage.WarpNpcCommand_NpcResult), npcName)).Append("\r\n");
                for (int i = 0; i < template.Maps.Length; i++)
                {
                    var mapId = template.Maps[i];
                    sb.Append($"\r\n#L{i}# {mapId} - {client.CurrentCulture.GetMapStreetName(mapId)} - {client.CurrentCulture.GetMapName(mapId)} #l");
                }
                (conversation ?? TempConversation.Create(client))?.RegisterSelect(sb.ToString(), (i, ctx) =>
                {
                    HandleMapNpc(client, template.Maps[i], npcId);
                });
            }
            else
            {
                HandleMapNpc(client, template.Maps[0], npcId);
            }
        }

        void HandleMapNpc(IChannelClient client, int mapId, int npcId)
        {
            if (mapId == client.OnlinedCharacter.getMapId())
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.AlreadyInMap));
                return;
            }
            var map = client.CurrentServer.getMapFactory().getMap(mapId);
            if (map == null)
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.MapNotFound), mapId.ToString());
                return;
            }

            var npc = map.getNPCById(npcId);
            if (npc == null)
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.NpcNotFoundInMap), npcId.ToString(), map.Id.ToString());
                return;
            }
            client.OnlinedCharacter.changeMap(map, npc.getPosition());
        }
    }
}
