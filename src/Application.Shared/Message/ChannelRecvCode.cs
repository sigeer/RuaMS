using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Shared.Message
{
    public class ChannelRecvCode
    {
        public const int RegisterChannel = 1;
        public const int UnregisterChannel = 2;
        public const int DisconnectAll = 3;

        public const int SaveAll = 4;

        public const int DropTextMessage = 7;
        public const int MultiChat = 8;
    }
}
