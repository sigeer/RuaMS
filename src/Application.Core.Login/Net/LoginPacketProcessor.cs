using Application.Core.Login.Client;
using Application.Core.Login.Net.Handlers;
using Application.Core.Net.Handlers;
using Microsoft.Extensions.DependencyInjection;
using net.server.handlers;
using net.server.handlers.login;

namespace Application.Core.Login.Net
{
    public class LoginPacketProcessor : IPacketProcessor<ILoginClient>
    {
        readonly Dictionary<short, IPacketHandlerBase<ILoginClient>> _dataSource;

        public LoginPacketProcessor(IServiceProvider sp)
        {
            _dataSource = new Dictionary<short, IPacketHandlerBase<ILoginClient>>
            {
                { (short)RecvOpcode.PONG, sp.GetRequiredService<KeepAliveHandler<ILoginClient>>() },
                { (short)RecvOpcode.CUSTOM_PACKET, sp.GetRequiredService<CustomPacketHandler<ILoginClient>>() },

                { (short)RecvOpcode.LOGIN_PASSWORD, sp.GetRequiredService<LoginPasswordHandler>() },
                { (short)RecvOpcode.ACCEPT_TOS, sp.GetRequiredService<AcceptToSHandler>() },
                { (short)RecvOpcode.AFTER_LOGIN, sp.GetRequiredService<AfterLoginHandler>() },
                { (short)RecvOpcode.SERVERLIST_REREQUEST, sp.GetRequiredService<ServerlistRequestHandler>() },
                { (short)RecvOpcode.CHARLIST_REQUEST, sp.GetRequiredService<CharlistRequestHandler>() },
                { (short)RecvOpcode.RELOG, sp.GetRequiredService<RelogRequestHandler>() },
                { (short)RecvOpcode.SERVERLIST_REQUEST, sp.GetRequiredService<ServerlistRequestHandler>() },
                { (short)RecvOpcode.SERVERSTATUS_REQUEST, sp.GetRequiredService<ServerStatusRequestHandler>() },
                { (short)RecvOpcode.CHECK_CHAR_NAME, sp.GetRequiredService<CheckCharNameHandler>() },
                { (short)RecvOpcode.CREATE_CHAR, sp.GetRequiredService<CreateCharHandler>() },
                { (short)RecvOpcode.DELETE_CHAR, sp.GetRequiredService<DeleteCharHandler>() },
                { (short)RecvOpcode.REGISTER_PIN, sp.GetRequiredService<RegisterPinHandler>() },
                { (short)RecvOpcode.REGISTER_PIC, sp.GetRequiredService<RegisterPicHandler>() },
                { (short)RecvOpcode.GUEST_LOGIN, sp.GetRequiredService<GuestLoginHandler>() },
                { (short)RecvOpcode.SET_GENDER, sp.GetRequiredService<SetGenderHandler>() },

                { (short)RecvOpcode.CHAR_SELECT, sp.GetRequiredService<CharSelectedHandler>() },
                { (short)RecvOpcode.CHAR_SELECT_WITH_PIC, sp.GetRequiredService<CharSelectedWithPicHandler>() },
                { (short)RecvOpcode.PICK_ALL_CHAR, sp.GetRequiredService<ViewAllCharSelectedHandler>() },
                { (short)RecvOpcode.VIEW_ALL_WITH_PIC, sp.GetRequiredService<ViewAllCharSelectedWithPicHandler>() },

                { (short)RecvOpcode.VIEW_ALL_CHAR, sp.GetRequiredService<ViewAllCharHandler>() },
                { (short)RecvOpcode.VIEW_ALL_PIC_REGISTER, sp.GetRequiredService<ViewAllCharRegisterPicHandler>() },
            };
        }

        public IPacketHandlerBase<ILoginClient>? GetPacketHandler(short code)
        {
            return _dataSource.GetValueOrDefault(code);
        }

        public void TryAddHandler(short code, IPacketHandlerBase<ILoginClient> handler)
        {
            _dataSource.TryAdd(code, handler);
        }
    }
}
