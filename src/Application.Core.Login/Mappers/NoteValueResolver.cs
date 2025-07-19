using Application.Core.Login.Models;
using Application.Shared.Constants;
using AutoMapper;
using ItemProto;

namespace Application.Core.Login.Mappers
{
    public class NoteSenderNameValueResolver : IValueResolver<NoteModel, Dto.NoteDto, string>
    {
        readonly MasterServer _server;

        public NoteSenderNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(NoteModel source, Dto.NoteDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.FromId)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }
    public class NoteReceiverNameValueResolver : IValueResolver<NoteModel, Dto.NoteDto, string>
    {
        readonly MasterServer _server;

        public NoteReceiverNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(NoteModel source, Dto.NoteDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.ToId)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }

}
