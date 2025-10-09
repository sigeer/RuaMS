using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm0;

public class MapOwnerClaimCommand : CommandBase
{
    public MapOwnerClaimCommand() : base(0, "mylawn")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;

                if (YamlConfig.config.server.USE_MAP_OWNERSHIP_SYSTEM)
                {
                    if (chr.getEventInstance() == null)
                    {
                        var map = chr.getMap();
                        if (map.countBosses() == 0)
                        {
                            // thanks Conrad for suggesting bosses prevent map leasing
                            var ownedMap = chr.getOwnedMap();  // thanks Conrad for suggesting not unlease a map as soon as player exits it
                            if (ownedMap != null)
                            {
                                ownedMap.unclaimOwnership(chr);

                                if (map == ownedMap)
                                {
                                    chr.dropMessage(5, c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.MapOwnerClaimCommand_Message1)));
                                    return;
                                }
                            }

                            if (map.claimOwnership(chr))
                            {
                                chr.dropMessage(5, c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.MapOwnerClaimCommand_Message2)));
                            }
                            else
                            {
                                chr.dropMessage(5, c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.MapOwnerClaimCommand_Message3)));
                            }
                        }
                        else
                        {
                            chr.dropMessage(5, c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.MapOwnerClaimCommand_Message4)));
                        }
                    }
                    else
                    {
                        chr.dropMessage(5, c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.MapOwnerClaimCommand_Message5)));
                    }
                }
                else
                {
                    chr.dropMessage(5, c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.FeatureUnavailable)));
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
