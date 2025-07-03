using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.BBS.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<BBSProto.BBSMainThread, BBSProto.BBSThreadPreview>()
                .ForMember(dest => dest.ReplyCount, src => src.MapFrom(x => x.Replies.Count));

            CreateMap<BbsThread, BBSThreadModel>();
            CreateMap<BbsReply, BBSReplyModel>();

            CreateMap<BBSThreadModel, BBSProto.BBSMainThread>();
            CreateMap<BBSReplyModel, BBSProto.BBSReplyDto>();
        }
    }
}
