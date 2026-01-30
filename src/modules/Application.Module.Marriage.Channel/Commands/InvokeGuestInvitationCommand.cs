using Application.Core.Channel.Commands;
using Application.Module.Marriage.Common.ErrorCodes;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeGuestInvitationCommand : IWorldChannelCommand
    {
        MarriageProto.InviteGuestResponse data;

        public InvokeGuestInvitationCommand(InviteGuestResponse data)
        {
            this.data = data;
        }

        public Task Execute(ChannelCommandContext ctx)
        {
            var code = (InviteErrorCode)data.Code;
            if (code != InviteErrorCode.Success)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.MasterId);
                if (masterChr != null)
                {
                    if (code == InviteErrorCode.GuestNotFound)
                    {
                        masterChr.dropMessage(5, "Unable to find " + data.Request.GuestName + "!");
                        return Task.CompletedTask;
                    }

                    if (code == InviteErrorCode.MarriageNotFound)
                    {
                        masterChr.dropMessage(5, $"Invitation was not sent to '{data.Request.GuestName}'. Either the time for your marriage reservation already came or it was not found.");
                        return Task.CompletedTask;
                    }

                    if (code == InviteErrorCode.DuplicateInvitation)
                    {
                        masterChr.dropMessage(5, $"'{data.Request.GuestName}' is already invited for your marriage.");
                        return Task.CompletedTask;
                    }

                    if (code == InviteErrorCode.WeddingUnderway)
                    {
                        masterChr.dropMessage(5, "Wedding is already under way. You cannot invite any more guests for the event.");
                        return Task.CompletedTask;
                    }

                    masterChr.GainItem(data.Request.ItemId, 1, false, false);
                }
                return Task.CompletedTask;
            }

            var guestChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.GuestId);
            if (guestChr != null && guestChr.isLoggedinWorld())
            {
                guestChr.dropMessage(6, $"[Wedding] You've been invited to {data.GroomName} and {data.BrideName}'s Wedding!");
            }
            return Task.CompletedTask;
        }
    }
}
