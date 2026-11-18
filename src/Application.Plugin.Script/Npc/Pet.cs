using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Shared.Constants.Inventory;
using Application.Templates.Item.Cash;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 1012005
        public async Task petmaster()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Intro));

            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Pet_Menu));

            switch (selection)
            {
                case 0:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_About));
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pet_Raise),
                        this.GetTalkMessage(ScriptMessage.Pet_Die)
                    ]);
                    break;
                case 1:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBrown));
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBrownPuppy),
                        this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsPink)
                    ]);
                    break;
                case 2:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsMiniKargo));
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsRudolph),
                        this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBlackPig)
                    ]);
                    break;
                case 3:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsPanda));
                    break;
                case 4:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsHusky));
                    break;
                case 5:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsDino));
                    break;
                case 6:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsMonkey));
                    break;
                case 7:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsTurkey));
                    break;
                case 8:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsWhite));
                    break;
                case 9:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsPenguin));
                    break;
                case 10:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsGolden));
                    break;
                case 11:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsRobot));
                    break;
                case 12:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsMini));
                    break;
                case 13:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsJr));
                    break;
                case 14:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBaby));
                    break;
                case 15:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsGreen));
                    break;
                case 16:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBlack));
                    break;
                case 17:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsJrReaper));
                    break;
                case 18:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsPorcupine));
                    break;
                case 19:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsSnowman));
                    break;
                case 20:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsSkunk));
                    break;
                case 21:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_TransferScroll));
                    break;
                case 22:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_ConfirmPurchase));
                    break;
                case 23:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_CheckMesoOrSlot));
                    break;
                case 24:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_LifeItem_Menu));
                    break;
                case 25:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_TakeLetter));
                    break;
                case 26:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_ObstacleIntro));
                    break;
                case 27:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_TooBusy));
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pet_HereIsLetter)))
                    {
                        if (getMeso() < 250000 || !canHold(4160011))
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Pet_NotMar));
                        }
                        else
                        {
                            await gainMeso(-250000);
                            await gainItem(4160011, 1);
                        }
                    }
                    break;
            }
        }


        // Npc: 1012006
        public async Task pet_lifeitem()
        {
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Pet_Frod_Letter));

            if (selection == 0)
            {
                if (haveItem(4031035))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Frod_NoPet));
                }
                else if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pet_Frod_Slacking)))
                {
                    await gainItem(4031035, 1);
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_HereIsLetter));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_TooBusy));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Pet_NotMar));
            }
        }


        // Npc: 1012007
        public async Task pet_letter()
        {
            if (haveItem(4031035))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Frod_Letter));
                if (getPlayer().getNoPets() == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pet_Frod_NoPet));
                }
                else
                {
                    await gainItem(4031035, -1);
                    await gainTameness(2);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pet_IntimacyUp));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Pet_Frod_Slacking));
            }
        }

        // Npc: 1032102
        public async Task pet_life()
        {
            var waterOfLife = 5180000;
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pet_ReviveOffer, npc, waterOfLife)))
            {
                if (!haveItem(waterOfLife))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pet_NoWaterOfLife, waterOfLife));
                    return;
                }

                var cashInv = getPlayer().getInventory(InventoryType.CASH);
                var canRevivePets = cashInv
                    .OfType<Pet>()
                    .Where(x => !x.SourceTemplate.NoRevive && x.getExpiration() == -1)
                    .ToList();
                if (canRevivePets.Count == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Shared_CurrentlyOwn));
                    return;
                }

                var index = await AskMenu(this.GetTalkMessage(ScriptMessage.Pet_PickRevive),
                    canRevivePets.Select(x => string.Format(this.GetTalkMessage(ScriptMessage.Pet_StatusLine), x.Name, x.Level, x.Tameness)));

                var item = canRevivePets[index];

                var waterInfo = ItemInformationProvider.getInstance().GetTemplate(waterOfLife) as WaterOfLifeItemTemplate;
                item.setExpiration(c.CurrentServer.Node.GetCurrentTimeDateTimeOffset().AddDays(waterInfo!.Life).ToUnixTimeMilliseconds());
                await getPlayer().forceUpdateItem(item);
                await getPlayer().GainItem(waterOfLife, -1);

                await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Revived));

            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Pet_SeeYou));
            }
        }

        // Npc: 2040030
        public async Task ludi026()
        {
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Pet2_Menu));

            switch (selection)
            {
                case 0:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Wisp, npc));
                    break;
                case 1:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pet2_About),
                        this.GetTalkMessage(ScriptMessage.Pet_Raise),
                        this.GetTalkMessage(ScriptMessage.Pet_Die)
                        ]);
                    break;
                case 2:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBrown),
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsBrown),
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsPink),
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsMiniCargo),
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsHusky)
                        ]);
                    break;
                case 3:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsBlack),
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsPanda),
                        this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsDino)
                        ]);
                    break;
                case 4:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsRudolph));
                    break;
                case 5:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsMonkey));
                    break;
                case 6:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsRobot));
                    break;
                case 7:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsElephant));
                    break;
                case 8:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsGolden));
                    break;
                case 9:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsPenguin));
                    break;
                case 10:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsMini));
                    break;
                case 11:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsJr));
                    break;
                case 12:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBaby));
                    break;
                case 13:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsGreen));
                    break;
                case 14:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsBlack));
                    break;
                case 15:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsSnowman));
                    break;
                case 16:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsSun));
                    break;
                case 17:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsJrReaper));
                    break;
                case 18:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsCrystal));
                    break;
                case 19:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsKino));
                    break;
                case 20:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsWhite));
                    break;
                case 21:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsPinkBean));
                    break;
                case 22:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsPorcupine));
                    break;
                case 23:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsSnowman));
                    break;
                case 24:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsSun));
                    break;
                case 25:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Cmd_CommandsJrReaper));
                    break;
                case 26:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsCrystal));
                    break;
                case 27:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsKino));
                    break;
                case 28:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsWhite));
                    break;
                case 29:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsPinkBean));
                    break;
                case 30:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet2_Cmd_CommandsPorcupine));
                    break;
            }
        }
        // Npc: 2040032
        public async Task ludi028()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pet_ObstacleIntro)))
            {
                if (haveItem(4031128))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_TakeLetter));
                }
                else
                {
                    await gainItem(4031128, 1);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pet_HereIsLetter));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Pet_TooBusy));
            }
        }
        // Npc: 2040033
        public async Task ludi029()
        {
            if (haveItem(4031128))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Frod_Letter));

                if (getPlayer().getNoPets() == 0)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_Frod_NoPet));
                }
                else
                {
                    await gainItem(4031128, -1);
                    await gainTameness(4);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pet_IntimacyUp));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Pet_Frod_Slacking));
            }
        }


        // Npc: 9102100
        public async Task multipet_success()
        {
            if (getQuestStatus(4646) == 1)
            {
                if (haveItem(4031921))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_FoundPoop));
                }
                else if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pet_PullOutQuestion)))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_FoundNote));
                    await gainItem(4031921, 1);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pet_HiddenItemIgnored));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Pet_NothingFound));
            }
        }


        // Npc: 9102101
        public async Task multipet_fail()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pet_PullOutQuestion)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Pet_YuckPoop));
                await gainItem(4031922, 1);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Pet_DidntTouch));
            }
        }
    }
}
