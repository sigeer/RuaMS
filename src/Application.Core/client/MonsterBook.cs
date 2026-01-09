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


using Google.Protobuf.Collections;
using tools;


namespace client;


public class MonsterBook
{
    public const int CardMaxCount = 5;

    private int specialCard = 0;
    private int normalCard = 0;
    private int bookLevel = 1;
    private Dictionary<int, int> cards = new();
    private Lock lockObj = new ();
    public Player Owner { get; }

    public MonsterBook(Player owner)
    {
        Owner = owner;
    }

    public void LoadData(RepeatedField<Dto.MonsterbookDto> dataList)
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
        lockObj.Enter();
        try
        {
            return cards.ToHashSet();
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public void addCard(int cardid)
    {
        Owner.getMap().broadcastMessage(Owner, PacketCreator.showForeignCardEffect(Owner.Id), false);

        int qty;
        lockObj.Enter();
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
            lockObj.Exit();
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
        lockObj.Enter();
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
            lockObj.Exit();
        }
    }

    public int getBookLevel()
    {
        lockObj.Enter();
        try
        {
            return bookLevel;
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public Dictionary<int, int> getCards()
    {
        lockObj.Enter();
        try
        {
            return new(cards);
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public int getTotalCards()
    {
        lockObj.Enter();
        try
        {
            return specialCard + normalCard;
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public int getNormalCard()
    {
        lockObj.Enter();
        try
        {
            return normalCard;
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public int getSpecialCard()
    {
        lockObj.Enter();
        try
        {
            return specialCard;
        }
        finally
        {
            lockObj.Exit();
        }
    }

    public Dto.MonsterbookDto[] ToDto()
    {
        return cards.Select(x => new Dto.MonsterbookDto()
        {
            Cardid = x.Key,
            Level = x.Value
        }).ToArray();
    }
}
