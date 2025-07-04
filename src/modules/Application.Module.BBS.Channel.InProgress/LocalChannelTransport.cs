using BBSProto;

namespace Application.Module.BBS.Channel.InProgress
{
    internal class LocalChannelTransport : IChannelTransport
    {
        readonly Master.BBSManager _manager;

        public LocalChannelTransport(Master.BBSManager manager)
        {
            _manager = manager;
        }

        public ShowBBSMainThreadResponse DeleteReply(DeleteReplyRequest deleteReplyRequest)
        {
            return _manager.DeleteBBSReply(deleteReplyRequest);
        }

        public void DeleteThread(DeleteThreadRequest deleteThreadRequest)
        {
            _manager.DeleteThread(deleteThreadRequest);
        }

        public ShowBBSMainThreadResponse EditThread(PostThreadRequest postThreadRequest)
        {
            return _manager.EditThread(postThreadRequest);
        }

        public ListBBSResponse ListThreads(ListBBSRequest listBBSRequest)
        {
            return _manager.ListBBSThreads(listBBSRequest);
        }

        public ShowBBSMainThreadResponse PostReply(PostReplyRequest postReplyRequest)
        {
            return _manager.PostReply(postReplyRequest);
        }

        public ShowBBSMainThreadResponse ShowThread(ShowThreadRequest showThreadRequest)
        {
            return _manager.ShowThread(showThreadRequest);
        }
    }
}
