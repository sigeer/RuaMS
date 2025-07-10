using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.BBS.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<BbsThreadEntity, BBSThreadModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Threadid))
                .ReverseMap()
                .ForMember(dest => dest.Threadid, src => src.MapFrom(x => x.Id));

            CreateMap<BbsReplyEntity, BBSReplyModel>()
                .ForMember(dest => dest.Threadid, src => src.MapFrom(x => x.Threadid));

            CreateMap<BBSThreadModel, BBSProto.BBSThreadDto>();
            CreateMap<BBSThreadModel, BBSProto.BBSThreadPreviewDto>()
                .ForMember(dest => dest.ReplyCount, src => src.MapFrom(x => x.Replies.Count));
            CreateMap<BBSReplyModel, BBSProto.BBSReplyDto>();
        }
    }
}
