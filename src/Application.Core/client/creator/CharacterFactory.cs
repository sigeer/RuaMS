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


using Application.Core.Managers;
using Application.Core.Managers.Constants;
using client.inventory;
using net.server;
using server;
using tools;

namespace client.creator;

/**
 * @author RonanLana
 */
public abstract class CharacterFactory
{
    static object createNewLock = new object();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="accountId"></param>
    /// <param name="name"></param>
    /// <param name="face"></param>
    /// <param name="hair"></param>
    /// <param name="skin"></param>
    /// <param name="gender"></param>
    /// <param name="recipe"></param>
    /// <param name="newchar"></param>
    /// <returns>0=成功</returns>
    public static int CreateCharacter(int accountId, string name, int face, int hair, int skin, int gender, CharacterFactoryRecipe recipe, out IPlayer? newCharacter)
    {
        lock (createNewLock)
        {
            newCharacter = null;
            if (!CharacterManager.CheckCharacterName(name))
            {
                return CreateCharResult.NameInvalid;
            }

            newCharacter = CharacterManager.NewPlayer(accountId);
            newCharacter.setSkinColor(SkinColorUtils.getById(skin));
            newCharacter.setGender(gender);
            newCharacter.setName(name);
            newCharacter.setHair(hair);
            newCharacter.setFace(face);

            newCharacter.setLevel(recipe.getLevel());
            newCharacter.setJob(recipe.getJob());
            newCharacter.Map = recipe.getMap();

            Inventory equipped = newCharacter.getInventory(InventoryType.EQUIPPED);
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            int top = recipe.getTop(), bottom = recipe.getBottom(), shoes = recipe.getShoes(), weapon = recipe.getWeapon();

            if (top > 0)
            {
                Item eq_top = ii.getEquipById(top);
                eq_top.setPosition(-5);
                equipped.addItemFromDB(eq_top);
            }

            if (bottom > 0)
            {
                Item eq_bottom = ii.getEquipById(bottom);
                eq_bottom.setPosition(-6);
                equipped.addItemFromDB(eq_bottom);
            }

            if (shoes > 0)
            {
                Item eq_shoes = ii.getEquipById(shoes);
                eq_shoes.setPosition(-7);
                equipped.addItemFromDB(eq_shoes);
            }

            if (weapon > 0)
            {
                Item eq_weapon = ii.getEquipById(weapon);
                eq_weapon.setPosition(-11);
                equipped.addItemFromDB(eq_weapon.copy());
            }

            if (!MakeCharInfoValidator.isNewCharacterValid(newCharacter))
            {
                Log.Logger.Warning("Owner from account {AccountId} tried to packet edit in character creation", newCharacter.getAccountID());
                return CreateCharResult.Error;
            }

            if (!newCharacter.insertNewChar(recipe))
            {
                return CreateCharResult.Error;
            }
            return CreateCharResult.Success;
        }
    }
    protected static int createNewCharacter(IChannelClient c, string name, int face, int hair, int skin, int gender, CharacterFactoryRecipe recipe)
    {
        lock (createNewLock)
        {
            if (c.GetAvailableCharacterSlots() <= 0)
            {
                return CreateCharResult.CharSlotLimited;
            }

            var result = CreateCharacter(c.AccountEntity!.Id, name, face, hair, skin, gender, recipe, out var newCharacter);
            if (result == CreateCharResult.Success && newCharacter != null)
            {
                newCharacter.setClient(c);
                c.sendPacket(PacketCreator.addNewCharEntry(c, newCharacter));

                Server.getInstance().createCharacterEntry(newCharacter);
                c.CurrentServer.BroadcastWorldGMPacket(PacketCreator.sendYellowTip("[New Char]: " + c.AccountEntity!.Name + " has created a new character with IGN " + name));
                Log.Logger.Information("Account {AccountName} created chr with name {CharacterName}", c.AccountEntity!.Name, name);

                return CreateCharResult.Success;
            }
            return result;
        }
    }
}
