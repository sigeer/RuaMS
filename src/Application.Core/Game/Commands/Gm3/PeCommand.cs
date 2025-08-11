using DotNetty.Buffers;
using System.Text.Json;


namespace Application.Core.Game.Commands.Gm3;

public class PeCommand : CommandBase
{
    readonly IPacketProcessor<IChannelClient> _processor;
    public PeCommand(IPacketProcessor<IChannelClient> processor) : base(3, "pe")
    {
        Description = "Handle synthesized packets from file, and handle them as if sent from a client";
        _processor = processor;
    }


    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        string packet = "";
        try
        {
            Dictionary<string, string>? packetProps = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("pe.text"));
            packet = packetProps?.GetValueOrDefault("pe") ?? "";
        }
        catch (IOException ex)
        {
            log.Error(ex.ToString());
            player.yellowMessage("Failed to load pe.txt");
            return;

        }
        byte[] packetContent = HexTool.toBytes(packet);
        InPacket inPacket = new ByteBufInPacket(Unpooled.WrappedBuffer(packetContent));
        short packetId = inPacket.readShort();
        var packetHandler = _processor.GetPacketHandler(packetId);
        if (packetHandler != null && packetHandler.ValidateState(c))
        {
            try
            {
                player.yellowMessage("Receiving: " + packet);
                packetHandler.HandlePacket(inPacket, c);
            }
            catch (Exception t)
            {
                string chrInfo = player != null ? player.getName() + " on map " + player.getMapId() : "?";
                log.Warning(t, "Error in packet handler {HandlerName}. Chr {CharacterName}, account {AccountName}. Packet: {Packet}", packetHandler.GetType().Name,
                        chrInfo, c.AccountEntity.Id, packet);
            }
        }
    }
}
