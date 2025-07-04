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


using Acornima.Ast;
using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.client.creator.novice;
using Application.Core.client.creator.veteran;
using Application.Core.Managers.Constants;
using Application.Core.Servers.Services;
using Application.Shared.Constants.Job;
using client.creator.novice;
using client.creator.veteran;
using client.inventory;
using constants.game;
using System.Reflection;
using System.Xml.Linq;
using tools;

namespace client.creator;

/**
 * @author RonanLana
 */
public abstract class CharacterFactory
{
    protected readonly ChannelService _channelService;
    object createNewLock = new object();

    protected CharacterFactory(ChannelService channelService)
    {
        _channelService = channelService;
    }


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
    private int CreateCharacter(int accountId, string name, int face, int hair, int skin, int gender, CharacterFactoryRecipe recipe)
    {
        lock (createNewLock)
        {
           var newCharacter = new Player(
                world: 0,
                accountId: accountId,
                hp: recipe.getMaxHp(),
                mp: recipe.getMaxMp(),
                str: recipe.getStr(),
                dex: recipe.getDex(),
                @int: recipe.getInt(),
                luk: recipe.getLuk(),
                job: recipe.getJob(),
                level: recipe.getLevel()
             );
            ;
            newCharacter.setSkinColor(SkinColorUtils.getById(skin));
            newCharacter.setGender(gender);
            newCharacter.setName(name);
            newCharacter.setHair(hair);
            newCharacter.setFace(face);

            newCharacter.setLevel(recipe.getLevel());
            newCharacter.setJob(recipe.getJob());
            newCharacter.Map = recipe.getMap();

            newCharacter.Ap = recipe.getRemainingAp();
            newCharacter.RemainingSp[GameConstants.getSkillBook(recipe.getJob().getId())] = recipe.getRemainingSp();
            newCharacter.MesoValue.set(recipe.getMeso());

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

            return _channelService.SendNewPlayer(newCharacter);
        }
    }
    protected int createNewCharacter(int accountId, string name, int face, int hair, int skin, int gender, CharacterFactoryRecipe recipe)
    {
        return CreateCharacter(accountId, name, face, hair, skin, gender, recipe);
    }

    public static NoviceCreator GetNoviceCreator(int type, ChannelService channelService)
    {
        if (type == 0) return new NoblesseCreator(channelService);
        if (type == 1) return new BeginnerCreator(channelService);
        if (type == 2) return new LegendCreator(channelService);
        throw new BusinessFatalException("不支持的创建类型");
    }

    public static VeteranCreator GetVeteranCreator(int type, ChannelService channelService)
    {
        return (type) switch
        {
            0 => new WarriorCreator(channelService),
            1 => new MagicianCreator(channelService),
            2 => new BowmanCreator(channelService),
            3 => new ThiefCreator(channelService),
            _ => new PirateCreator(channelService),
        };
    }
}
