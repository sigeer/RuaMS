using Application.EF.Entities;
using Mapster;

namespace Application.Module.BBS.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<BbsThreadEntity, BBSThreadModel>()
                .TwoWays()
                .Map(dest => dest.Id, x => x.Threadid);

            config.NewConfig<BbsReplyEntity, BBSReplyModel>();

            config.NewConfig<BBSThreadModel, BBSProto.BBSThreadDto>();
            config.NewConfig<BBSThreadModel, BBSProto.BBSThreadPreviewDto>()
                .Map(dest => dest.ReplyCount, x => x.Replies.Count);
            config.NewConfig<BBSReplyModel, BBSProto.BBSReplyDto>();
        }
    }
}
