using client;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server.coordinator.world;
using tools;

namespace net.server.channel.handlers;

public class AcceptFamilyHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
        var chr = c.OnlinedCharacter;
        int inviterId = p.readInt();
        p.readString();
        bool accept = p.readByte() != 0;
        // string inviterName = slea.readMapleAsciiString();
        var inviter = c.getWorldServer().getPlayerStorage().getCharacterById(inviterId);
        if (inviter != null && inviter.IsOnlined)
        {
            InviteResult inviteResult = InviteCoordinator.answerInvite(InviteType.FAMILY, c.OnlinedCharacter.getId(), c.OnlinedCharacter, accept);
            if (inviteResult.result == InviteResultType.NOT_FOUND)
            {
                return; //was never invited. (or expired on server only somehow?)
            }
            if (accept)
            {
                var inviterFamily = inviter.getFamily();
                if (inviterFamily != null)
                {
                    if (chr.getFamily() == null)
                    {
                        var newEntry = new FamilyEntry(inviterFamily, chr.getId(), chr.getName(), chr.getLevel(), chr.getJob());
                        newEntry.setCharacter(chr);
                        if (!newEntry.setSenior(inviter.getFamilyEntry(), true))
                        {
                            inviter.sendPacket(PacketCreator.sendFamilyMessage(1, 0));
                            return;
                        }
                        else
                        {
                            // save
                            inviterFamily.addEntry(newEntry);
                            insertNewFamilyRecord(chr.getId(), inviterFamily.getID(), inviter.getId(), false);
                        }
                    }
                    else
                    {
                        //absorb target family
                        var targetEntry = chr.getFamilyEntry();
                        Family targetFamily = targetEntry.getFamily();
                        if (targetFamily.getLeader() != targetEntry)
                        {
                            return;
                        }
                        if (inviterFamily.getTotalGenerations() + targetFamily.getTotalGenerations() <= YamlConfig.config.server.FAMILY_MAX_GENERATIONS)
                        {
                            targetEntry.join(inviter.getFamilyEntry());
                        }
                        else
                        {
                            inviter.sendPacket(PacketCreator.sendFamilyMessage(76, 0));
                            chr.sendPacket(PacketCreator.sendFamilyMessage(76, 0));
                            return;
                        }
                    }
                }
                else
                {
                    var chrFamily = chr.getFamily();
                    // create new family
                    // !!! inviterFamily在此处为null
                    if (chrFamily != null
                        && inviterFamily != null
                        && chrFamily.getTotalGenerations() + inviterFamily.getTotalGenerations() >= YamlConfig.config.server.FAMILY_MAX_GENERATIONS)
                    {
                        var message = PacketCreator.sendFamilyMessage(76, 0);
                        inviter.sendPacket(message);
                        chr.sendPacket(message);
                        return;
                    }
                    Family newFamily = new Family(-1, c.getWorld());
                    c.getWorldServer().addFamily(newFamily.getID(), newFamily);
                    FamilyEntry inviterEntry = new FamilyEntry(newFamily, inviter.getId(), inviter.getName(), inviter.getLevel(), inviter.getJob());
                    inviterEntry.setCharacter(inviter);
                    newFamily.setLeader(inviter.getFamilyEntry());
                    newFamily.addEntry(inviterEntry);
                    if (chr.getFamily() == null)
                    { //completely new family
                        FamilyEntry newEntry = new FamilyEntry(newFamily, chr.getId(), chr.getName(), chr.getLevel(), chr.getJob());
                        newEntry.setCharacter(chr);
                        newEntry.setSenior(inviterEntry, true);
                        // save new family
                        insertNewFamilyRecord(inviter.getId(), newFamily.getID(), 0, true);
                        insertNewFamilyRecord(chr.getId(), newFamily.getID(), inviter.getId(), false); // char was already saved from setSenior() above
                        newFamily.setMessage("", true);
                    }
                    else
                    { //new family for inviter, absorb invitee family
                        insertNewFamilyRecord(inviter.getId(), newFamily.getID(), 0, true);
                        newFamily.setMessage("", true);
                        chr.getFamilyEntry().join(inviterEntry);
                    }
                }
                chr.getFamily().broadcast(PacketCreator.sendFamilyJoinResponse(true, chr.getName()), chr.getId());
                c.sendPacket(PacketCreator.getSeniorMessage(inviter.getName()));
                c.sendPacket(PacketCreator.getFamilyInfo(chr.getFamilyEntry()));
                chr.getFamilyEntry().updateSeniorFamilyInfo(true);
            }
            else
            {
                inviter.sendPacket(PacketCreator.sendFamilyJoinResponse(false, chr.getName()));
            }
        }
        c.sendPacket(PacketCreator.sendFamilyMessage(0, 0));
    }

    private void insertNewFamilyRecord(int characterID, int familyID, int seniorID, bool updateChar)
    {
        try
        {
            using var dbContext = new DBContext();

            try
            {
                var newModel = new FamilyCharacter(characterID, familyID, seniorID);
                dbContext.FamilyCharacters.Add(newModel);
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                log.Error(e, "Could not save new family record for chrId {CharacterId}", characterID);
            }
            if (updateChar)
            {
                try
                {
                    dbContext.Characters.Where(x => x.Id == characterID).ExecuteUpdate(x => x.SetProperty(y => y.FamilyId, familyID));
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not update 'characters' 'familyid' record for chrId {CharacterId}", characterID);
                }
            }
        }
        catch (Exception e)
        {
            log.Error(e, "Could not get connection to DB while inserting new family record");
        }
    }
}
