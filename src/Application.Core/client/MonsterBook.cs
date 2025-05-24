/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

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


using Application.Shared.Characters;
using Microsoft.EntityFrameworkCore;
using tools;


namespace client;


public class MonsterBook
{
    public const int CardMaxCount = 5;

    private int specialCard = 0;
    private int normalCard = 0;
    private int bookLevel = 1;
    private Dictionary<int, int> cards = new();
    private object lockObj = new object();
    public IPlayer Owner { get; }

    public MonsterBook(IPlayer owner)
    {
        Owner = owner;
    }

    public void LoadData(MonsterbookDto[] dataList)
    {
        foreach (var item in dataList)
        {
            var cardid = item.Cardid;
            var level = item.Level;
            if (cardid / 1000 >= 2388)
            {
                specialCard++;
            }
            else
            {
                normalCard++;
            }
            cards[cardid] = level;
        }
        calculateLevel();
    }

    public HashSet<KeyValuePair<int, int>> getCardSet()
    {
        Monitor.Enter(lockObj);
        try
        {
            return cards.ToHashSet();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void addCard(int cardid)
    {
        Owner.getMap().broadcastMessage(Owner, PacketCreator.showForeignCardEffect(Owner.Id), false);

        int qty;
        Monitor.Enter(lockObj);
        try
        {
            qty = cards.GetValueOrDefault(cardid);

            if (qty < CardMaxCount)
            {
                cards.AddOrUpdate(cardid, qty + 1);
            }

            if (qty == 0)
            {
                if (cardid / 1000 >= 2388)
                {
                    specialCard++;
                }
                else
                {
                    normalCard++;
                }
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        if (qty < CardMaxCount)
        {
            if (qty == 0)
            {
                // leveling system only accounts unique cards
                calculateLevel();
            }

            Owner.sendPacket(PacketCreator.addCard(false, cardid, qty + 1));
            Owner.sendPacket(PacketCreator.showGainCard());
        }
        else
        {
            Owner.sendPacket(PacketCreator.addCard(true, cardid, CardMaxCount));
        }
    }

    private void calculateLevel()
    {
        Monitor.Enter(lockObj);
        try
        {
            int collectionExp = (normalCard + specialCard);

            int level = 0, expToNextlevel = 1;
            do
            {
                level++;
                expToNextlevel += level * 10;
            } while (collectionExp >= expToNextlevel);

            bookLevel = level;  // thanks IxianMace for noticing book level differing between book UI and character info UI
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public int getBookLevel()
    {
        Monitor.Enter(lockObj);
        try
        {
            return bookLevel;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public Dictionary<int, int> getCards()
    {
        Monitor.Enter(lockObj);
        try
        {
            return new(cards);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public int getTotalCards()
    {
        Monitor.Enter(lockObj);
        try
        {
            return specialCard + normalCard;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public int getNormalCard()
    {
        Monitor.Enter(lockObj);
        try
        {
            return normalCard;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public int getSpecialCard()
    {
        Monitor.Enter(lockObj);
        try
        {
            return specialCard;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void loadCards(DBContext dbContext, int charid)
    {
        Monitor.Enter(lockObj);
        try
        {
            var dataList = dbContext.Monsterbooks.Where(x => x.Charid == charid).OrderBy(x => x.Cardid).Select(x => new { x.Cardid, x.Level }).ToList();
            foreach (var item in dataList)
            {
                var cardid = item.Cardid;
                var level = item.Level;
                if (cardid / 1000 >= 2388)
                {
                    specialCard++;
                }
                else
                {
                    normalCard++;
                }
                cards.AddOrUpdate(cardid, level);
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        calculateLevel();
    }

    public void saveCards(DBContext dbContext, int chrId)
    {
        try
        {
            dbContext.Monsterbooks.Where(x => x.Charid == chrId).ExecuteDelete();
            dbContext.Monsterbooks.AddRange(cards.Select(x => new MonsterbookEntity(chrId, x.Key, x.Value)));
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public MonsterbookDto[] ToDto()
    {
        return cards.Select(x => new MonsterbookDto()
        {
            Cardid = x.Key,
            Level = x.Value
        }).ToArray();
    }

    public static int[] getCardTierSize()
    {
        try
        {
            using var dbContext = new DBContext();
            return dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM monstercarddata GROUP BY floor(cardid / 1000);").ToArray();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return new int[0];
        }
    }
}
