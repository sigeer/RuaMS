using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.BBS.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<BbsThreadEntity, BBSThreadModel>()
                .ForMember(dest => dest.ThreadId, src => src.MapFrom(x => x.Threadid));
            CreateMap<BbsReplyEntity, BBSReplyModel>()
                .ForMember(dest => dest.Threadid, src => src.MapFrom(x => x.Threadid));

            CreateMap<BBSThreadModel, BBSProto.BBSMainThread>();
            CreateMap<BBSThreadModel, BBSProto.BBSThreadPreview>()
                .ForMember(dest => dest.ReplyCount, src => src.MapFrom(x => x.Replies.Count));
            CreateMap<BBSReplyModel, BBSProto.BBSReplyDto>();
        }
    }
}
