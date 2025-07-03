using BBSProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.BBS.Channel
{
    public interface IChannelTransport
    {
        void DeleteReply(DeleteReplyRequest deleteReplyRequest);
        void DeleteThread(DeleteThreadRequest deleteThreadRequest);
        void EditThread(PostThreadRequest postThreadRequest);
        void ListThreads(ListBBSRequest listBBSRequest);
        void PostReply(PostReplyRequest postReplyRequest);
        void ShowThread(ShowThreadRequest showThreadRequest);
    }
}
