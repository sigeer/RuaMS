using BBSProto;

namespace Application.Module.BBS.Channel
{
    public interface IChannelTransport
    {
        ShowBBSMainThreadResponse DeleteReply(DeleteReplyRequest deleteReplyRequest);
        void DeleteThread(DeleteThreadRequest deleteThreadRequest);
        ShowBBSMainThreadResponse EditThread(PostThreadRequest postThreadRequest);
        BBSProto.ListBBSResponse ListThreads(ListBBSRequest listBBSRequest);
        ShowBBSMainThreadResponse PostReply(PostReplyRequest postReplyRequest);
        ShowBBSMainThreadResponse ShowThread(ShowThreadRequest showThreadRequest);
    }
}
