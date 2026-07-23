using Application.EF.Entities;
using Mapster;

namespace Application.Module.BBS.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<BbsThreadEntity, BBSThreadModel>()
                .Map(dest => dest.Id, src => src.Threadid);

            config.NewConfig<BBSThreadModel, BbsThreadEntity>()
                .Map(dest => dest.Threadid, src => src.Id);

            config.NewConfig<BBSThreadModel, BBSProto.BBSThreadDto>();
            config.NewConfig<BBSThreadModel, BBSProto.BBSThreadPreviewDto>()
                .Map(dest => dest.ReplyCount, src => src.Replies.Count);
            config.NewConfig<BBSReplyModel, BBSProto.BBSReplyDto>();
        }
    }
}
