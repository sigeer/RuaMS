using Application.Core.ServerTransports;
using Application.Shared.Invitations;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Channel.ServerData
{
    public class TradeService
    {
        readonly WorldChannelServer _server;
        readonly IChannelServerTransport _transport;
        readonly IMapper _mapper;

        public void CreateInvite(IPlayer chr, string toName)
        {
            _transport.SendInvitation(new Dto.CreateInviteRequest { FromId = chr.Id, ToName = toName, Type = InviteTypes.Trade });
        }
    }
}
