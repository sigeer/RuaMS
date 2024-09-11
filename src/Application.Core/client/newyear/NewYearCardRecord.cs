/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.scripting.Event;
using Microsoft.EntityFrameworkCore;
using net.server;
using server;
using tools;

namespace client.newyear;

/**
 * @author Ronan - credits to Eric for showing the New Year opcodes and handler layout
 */
public class NewYearCardRecord
{
    private int id;

    private int senderId;
    private string senderName;
    private bool senderDiscardCard;

    private int receiverId;
    private string receiverName;
    private bool receiverDiscardCard;
    private bool receiverReceivedCard;

    private string stringContent;
    private long dateSent = 0;
    private long dateReceived = 0;

    private ScheduledFuture? sendTask = null;

    public NewYearCardRecord(int senderid, string sender, int receiverid, string receiver, string message)
    {
        this.id = -1;

        this.senderId = senderid;
        this.senderName = sender;
        this.senderDiscardCard = false;

        this.receiverId = receiverid;
        this.receiverName = receiver;
        this.receiverDiscardCard = false;
        this.receiverReceivedCard = false;

        this.stringContent = message;

        this.dateSent = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        this.dateReceived = 0;
    }

    private void setExtraNewYearCardRecord(int id, bool senderDiscardCard, bool receiverDiscardCard, bool receiverReceivedCard, long dateSent, long dateReceived)
    {
        this.id = id;
        this.senderDiscardCard = senderDiscardCard;
        this.receiverDiscardCard = receiverDiscardCard;
        this.receiverReceivedCard = receiverReceivedCard;

        this.dateSent = dateSent;
        this.dateReceived = dateReceived;
    }

    public void setId(int cardid)
    {
        this.id = cardid;
    }

    public int getId()
    {
        return this.id;
    }

    public int getSenderId()
    {
        return senderId;
    }

    public string getSenderName()
    {
        return senderName;
    }

    public bool isSenderCardDiscarded()
    {
        return senderDiscardCard;
    }

    public int getReceiverId()
    {
        return receiverId;
    }

    public string getReceiverName()
    {
        return receiverName;
    }

    public bool isReceiverCardDiscarded()
    {
        return receiverDiscardCard;
    }

    public bool isReceiverCardReceived()
    {
        return receiverReceivedCard;
    }

    public string getMessage()
    {
        return stringContent;
    }

    public long getDateSent()
    {
        return dateSent;
    }

    public long getDateReceived()
    {
        return dateReceived;
    }

    public static void saveNewYearCard(NewYearCardRecord newyear)
    {
        using var dbContext = new DBContext();
        var newModel = new Newyear
        {
            SenderId = newyear.senderId,
            ReceiverId = newyear.receiverId,
            SenderName = newyear.senderName,
            ReceiverName = newyear.receiverName,
            SenderDiscard = newyear.senderDiscardCard,
            ReceiverDiscard = newyear.receiverDiscardCard,
            Received = newyear.receiverReceivedCard,
            TimeSent = newyear.dateSent,
            TimeReceived = newyear.dateReceived
        };
        dbContext.Newyears.Add(newModel);
        dbContext.SaveChanges();
        newyear.id = newModel.Id;

    }

    public static void updateNewYearCard(NewYearCardRecord newyear)
    {
        newyear.receiverReceivedCard = true;
        newyear.dateReceived = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        using var dbContext = new DBContext();
        dbContext.Newyears.Where(x => x.Id == newyear.id)
            .ExecuteUpdate(x => x.SetProperty(y => y.TimeReceived, newyear.dateReceived).SetProperty(y => y.Received, true));
    }

    public static NewYearCardRecord? loadNewYearCard(int cardid)
    {
        NewYearCardRecord? nyc = Server.getInstance().getNewYearCard(cardid);
        if (nyc != null)
            return nyc;

        using var dbConext = new DBContext();
        var newyear = dbConext.Newyears.Where(x => x.Id == cardid).ToList().Select(x =>
        {
            var m = new NewYearCardRecord(x.SenderId, x.SenderName, x.ReceiverId, x.ReceiverName, x.Message);
            m.setExtraNewYearCardRecord(x.Id, x.SenderDiscard, x.ReceiverDiscard, x.Received, x.TimeSent, x.TimeReceived);
            return m;
        }).FirstOrDefault();
        if (newyear != null)
            Server.getInstance().setNewYearCard(newyear);
        return newyear;
    }

    public static void loadPlayerNewYearCards(IPlayer chr)
    {
        using var dbConext = new DBContext();
        var ds = dbConext.Newyears.Where(x => x.SenderId == chr.getId() || x.ReceiverId == chr.getId()).ToList().Select(x =>
        {
            var m = new NewYearCardRecord(x.SenderId, x.SenderName, x.ReceiverId, x.ReceiverName, x.Message);
            m.setExtraNewYearCardRecord(x.Id, x.SenderDiscard, x.ReceiverDiscard, x.Received, x.TimeSent, x.TimeReceived);
            chr.addNewYearRecord(m);
            return m;
        }).ToList();
    }

    public static void printNewYearRecords(IPlayer chr)
    {
        chr.dropMessage(5, "New Years: " + chr.getNewYearRecords().Count);

        foreach (NewYearCardRecord nyc in chr.getNewYearRecords())
        {
            chr.dropMessage(5, "-------------------------------");

            chr.dropMessage(5, "Id: " + nyc.id);

            chr.dropMessage(5, "Sender id: " + nyc.senderId);
            chr.dropMessage(5, "Sender name: " + nyc.senderName);
            chr.dropMessage(5, "Sender discard: " + nyc.senderDiscardCard);

            chr.dropMessage(5, "Receiver id: " + nyc.receiverId);
            chr.dropMessage(5, "Receiver name: " + nyc.receiverName);
            chr.dropMessage(5, "Receiver discard: " + nyc.receiverDiscardCard);
            chr.dropMessage(5, "Received: " + nyc.receiverReceivedCard);

            chr.dropMessage(5, "Message: " + nyc.stringContent);
            chr.dropMessage(5, "Date sent: " + nyc.dateSent);
            chr.dropMessage(5, "Date recv: " + nyc.dateReceived);
        }
    }

    public void startNewYearCardTask()
    {
        if (sendTask != null)
        {
            return;
        }

        sendTask = TimerManager.getInstance().register(() =>
        {
            Server server = Server.getInstance();

            int world = server.getCharacterWorld(receiverId);
            if (world == -1)
            {
                sendTask!.cancel(false);
                sendTask = null;

                return;
            }

            var target = server.getWorld(world).getPlayerStorage().getCharacterById(receiverId);
            if (target != null && target.isLoggedinWorld())
            {
                target.sendPacket(PacketCreator.onNewYearCardRes(target, this, 0xC, 0));
            }
        }, TimeSpan.FromHours(1));
    }

    public void stopNewYearCardTask()
    {
        if (sendTask != null)
        {
            sendTask.cancel(false);
            sendTask = null;
        }
    }

    private static void deleteNewYearCard(int id)
    {
        Server.getInstance().removeNewYearCard(id);

        using var dbConext = new DBContext();
        dbConext.Newyears.Where(x => x.Id == id).ExecuteDelete();
    }

    public static void removeAllNewYearCard(bool send, IPlayer chr)
    {
        int cid = chr.getId();

        /* not truly needed since it's going to be hard removed from the DB
        string actor = (send ? "sender" : "receiver");
        
        try (DBContext dbContext = DatabaseConnection.getConnection()) {
            try (PreparedStatement ps = con.prepareStatement("UPDATE newyear SET " + actor + "id = 1, received = 0 WHERE " + actor + "id = ?")) {
                ps.setInt(1, cid);
                ps.executeUpdate();
            }
        } catch(SQLException sqle) {
            sqlLog.Logger.Error(e.ToString());
        }
        */

        HashSet<NewYearCardRecord> set = new(chr.getNewYearRecords());
        foreach (NewYearCardRecord nyc in set)
        {
            if (send)
            {
                if (nyc.senderId == cid)
                {
                    nyc.senderDiscardCard = true;
                    nyc.receiverReceivedCard = false;

                    chr.removeNewYearRecord(nyc);
                    deleteNewYearCard(nyc.id);

                    chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, nyc, 0xE, 0));

                    var other = chr.getClient().getWorldServer().getPlayerStorage().getCharacterById(nyc.getReceiverId());
                    if (other != null && other.isLoggedinWorld())
                    {
                        other.removeNewYearRecord(nyc);
                        other.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(other, nyc, 0xE, 0));

                        other.dropMessage(6, "[New Year] " + chr.getName() + " threw away the New Year card.");
                    }
                }
            }
            else
            {
                if (nyc.receiverId == cid)
                {
                    nyc.receiverDiscardCard = true;
                    nyc.receiverReceivedCard = false;

                    chr.removeNewYearRecord(nyc);
                    deleteNewYearCard(nyc.id);

                    chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, nyc, 0xE, 0));

                    var other = chr.getClient().getWorldServer().getPlayerStorage().getCharacterById(nyc.getSenderId());
                    if (other != null && other.isLoggedinWorld())
                    {
                        other.removeNewYearRecord(nyc);
                        other.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(other, nyc, 0xE, 0));

                        other.dropMessage(6, "[New Year] " + chr.getName() + " threw away the New Year card.");
                    }
                }
            }
        }
    }

    public static void startPendingNewYearCardRequests(DBContext dbContext)
    {
        dbContext.Newyears.Where(x => x.TimeReceived == 0 && !x.SenderDiscard).ToList()
            .ForEach(x =>
            {
                var newyear = new NewYearCardRecord(x.SenderId, x.SenderName, x.ReceiverId, x.ReceiverName, x.Message);
                newyear.setExtraNewYearCardRecord(x.Id, x.SenderDiscard, x.ReceiverDiscard, x.Received, x.TimeSent, x.TimeReceived);
                Server.getInstance().setNewYearCard(newyear);
                newyear.startNewYearCardTask();

            });
    }
}
