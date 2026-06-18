using Application.Core.Game.Items;
using Application.Templates.Item.Pet;
using Application.Utility.Tickables;
using server.movement;
using tools;

namespace Application.Core.Game.Maps.AnimatedObjects
{
    public class MapPet : AbstractAnimatedMapObject, ILoopTickable
    {
        public Player Owner => PetItem.PlayerInventory!.Owner;
        public Pet PetItem { get; }
        public int Fullness { get => PetItem.Fullness; set => PetItem.Fullness = value; }
        public int Tameness { get => PetItem.Tameness; set => PetItem.Tameness = value; }
        public byte Level { get => PetItem.Level; set => PetItem.Level = value; }
        public string Name { get => PetItem.Name; set => PetItem.Name = value; }
        public PetItemTemplate SourceTemplate => PetItem.SourceTemplate;
        public sbyte Index => Owner.getPetIndex(PetId);

        public MapPet(Pet sourceItem)
            : base(
                sourceItem.PlayerInventory!.Owner.MapModel,
                sourceItem.PlayerInventory!.Owner.MapModel.getGroundBelow(
                    sourceItem.PlayerInventory!.Owner.getPosition()
                ),
                0
            )
        {
            PetItem = sourceItem;
        }

        public override Player? Controller => Owner;

        public override MapObjectType getType()
        {
            return MapObjectType.PET;
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetReadableName(IChannelClient c)
        {
            return base.GetReadableName(c) + $" Owner {Owner.GetReadableName(c)}";
        }

        public override async Task sendSpawnData(IChannelClient client)
        {
            await client.SendPacket(EncodeShowPet());
        }

        public override async Task sendDestroyData(IChannelClient client)
        {
            await client.SendPacket(EncodeHidePet(0));
        }

        public bool HasChatBalloon => Owner.GetEquipped().HasEquipped(EquipSlot.PetEquipSlots[Index].ChatBalloon);
        public bool HasNameTag => Owner.GetEquipped().HasEquipped(EquipSlot.PetEquipSlots[Index].NameTag);

        public override async Task OnMounted(IMap map)
        {
            await base.OnMounted(map);
            setPosition(Owner.getPosition());
        }

        public short GetFoothold() => (short)MapModel.Footholds.FindBelowFoothold(getPosition())!.getId();

        public override bool IsVisibleForPlayer(Player chr)
        {
            return Owner == chr || base.IsVisibleForPlayer(chr) && !chr.HidePet;
        }

        public void EncodeData(OutPacket p)
        {
            p.writeInt(PetItem.getItemId());
            p.writeString(PetItem.Name);
            p.writeLong(PetId);
            p.writePos(getPosition());
            p.writeByte(getStance());
            p.writeShort(GetFoothold()); // fh
            p.writeBool(HasNameTag); // nameTag
            p.writeBool(HasChatBalloon); // chatBalloon
        }

        Packet EncodeHidePet(byte recallReason)
        {
            // CUserLocal::OnPetActivated
            OutPacket p = OutPacket.create(SendOpcode.SPAWN_PET);
            p.writeInt(Owner.Id);
            p.writeByte(Index);
            p.writeByte(0);
            p.writeByte(recallReason);
            return p;
        }

        Packet EncodeShowPet()
        {
            OutPacket p = OutPacket.create(SendOpcode.SPAWN_PET);
            p.writeInt(Owner.Id);
            p.writeByte(Index);
            p.writeByte(1);
            //   if ( CInPacket::Decode1(a2) )
            //     CUser::SetActivePet(v2, v3, 0);
            p.writeByte(0);
            EncodeData(p);
            return p;
        }

        Packet EncodeFoodResponse(bool success)
        {
            OutPacket p = OutPacket.create(SendOpcode.PET_COMMAND);
            p.writeInt(Owner.Id);
            p.writeSByte(Index);
            p.writeByte(1);
            p.writeBool(success);
            p.writeBool(HasChatBalloon);
            return p;
        }

        Packet EncodeCommandResponse(int command, bool success)
        {
            OutPacket p = OutPacket.create(SendOpcode.PET_COMMAND);
            p.writeInt(Owner.Id);
            p.writeSByte(Index);
            p.writeByte(0);
            p.writeByte(command);
            p.writeBool(success);
            p.writeBool(HasChatBalloon);
            return p;
        }

        public async Task ActionRemote(sbyte act, string text)
        {
            OutPacket p = OutPacket.create(SendOpcode.PET_ACTION);
            p.writeInt(Owner.Id);
            p.writeSByte(Index);
            p.writeByte(0); // nType
            p.writeByte(act); // nAction
            p.writeString(text);
            p.writeBool(HasChatBalloon); // bChatBalloon

            await BroadcastMap(p, Owner.Id);
        }

        public async Task BroadcastNameChanged(bool exceptOwer = true)
        {
            OutPacket p = OutPacket.create(SendOpcode.PET_NAMECHANGE);
            p.writeInt(Owner.Id);
            p.writeByte(Index);
            p.writeString(Name);

            //   if ( CInPacket::Decode1(v3) )
            //     nNameTag = this->m_pTemplate->nNameTag;
            p.writeBool(HasNameTag);

            await BroadcastMap(p, exceptOwer ? Owner.Id : -1);
        }

        public async Task UpdateName(string name)
        {
            PetItem.Name = name;

            await Owner.forceUpdateItem(PetItem);
            await BroadcastNameChanged(false);
        }
        /// <summary>
        /// 召回
        /// </summary>
        /// <param name="recallReason">0. 无，1. 饱食度过低 2. 过期</param>
        public async Task Recall(byte recallReason = 0)
        {
            await MapModel.RemoveMapObject(this, mapChr => mapChr.SendPacket(EncodeHidePet(recallReason)));

            Owner.removePet(PetId, true);
            await Owner.commitExcludedItems();

            await Owner.SendPacket(PacketCreator.petStatUpdate(Owner));
            await Owner.SendPacket(PacketCreator.enableActions());
        }

        public int getItemId()
        {
            return PetItem.getItemId();
        }

        public long PetId => PetItem.PetId;

        public long getUniqueId()
        {
            return PetItem.getUniqueId();
        }

        public string getName()
        {
            return PetItem.Name;
        }


        public async Task gainTamenessFullness(int incTameness, int incFullness, int type, bool forceEnjoy = false)
        {
            bool enjoyed;

            //will NOT increase pet's tameness if tried to feed pet with 100% fullness
            // unless forceEnjoy == true (cash shop)
            if (Fullness < Pet.MaxFullness || incFullness == 0 || forceEnjoy)
            {
                //incFullness == 0: command given
                int newFullness = Math.Min(Fullness + incFullness, Pet.MaxFullness);
                Fullness = newFullness;

                if (incTameness > 0 && Tameness < Pet.MaxTameness)
                {
                    int newTameness = Math.Min(Tameness + incTameness, Pet.MaxTameness);
                    Tameness = newTameness;
                    while (newTameness >= ExpTable.getTamenessNeededForLevel(Level))
                    {
                        Level += 1;

                        await Owner.SendPacket(PacketCreator.showOwnPetLevelUp(Index));
                        await BroadcastMap(PacketCreator.showPetLevelUp(Owner, Index), Owner.Id);
                    }
                }

                enjoyed = true;
            }
            else
            {
                int newTameness = Tameness - 1;
                if (newTameness < 0)
                {
                    newTameness = 0;
                }

                Tameness = newTameness;
                if (Level > 1 && newTameness < ExpTable.getTamenessNeededForLevel(Level - 1))
                {
                    Level -= 1;
                }

                enjoyed = false;
            }

            if (forceEnjoy)
            {
                await Owner.SendPacket(EncodeFoodResponse(true));
                // 没观察到任何效果
                await Owner.SendPacket(PacketCreator.PetEatCashFoodSuccess(Index));
            }
            else
            {
                await Owner.SendPacket(EncodeFoodResponse(enjoyed));
            }


            await Owner.forceUpdateItem(PetItem);
        }

        public async Task HandleCommand(byte command)
        {
            var petCommand = SourceTemplate.InterActsDict.GetValueOrDefault(command);
            if (petCommand == null)
            {
                return;
            }

            // 客户端再根据成功/失败触发petchat
            if (Randomizer.nextInt(100) < petCommand.Prob)
            {
                await gainTamenessFullness(petCommand.Inc, 0, command);
                await Owner.SendPacket(EncodeCommandResponse(command, true));
            }
            else
            {
                await Owner.SendPacket(EncodeCommandResponse(command, false));
            }
        }


        public void updatePosition(List<LifeMovementFragment> movement)
        {
            foreach (LifeMovementFragment move in movement)
            {
                if (move is LifeMovement lifeMovement)
                {
                    if (move is AbsoluteLifeMovement)
                    {
                        setPosition(move.getPosition());
                    }
                    setStance(lifeMovement.getNewstate());
                }
            }
        }

        int step = 0;
        public long Period => 60_000;

        public long Next { get; private set; }

        public TickableStatus Status { get; private set; }
        public async Task OnTick(long now)
        {
            if (!this.IsAvailable() || PetItem.PlayerInventory == null)
            {
                return;
            }

            if (PetItem.PlayerInventory.Owner.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
            {
                return;
            }

            if (Status == TickableStatus.Active && Next >= now)
            {
                if (step % YamlConfig.config.server.PET_EXHAUST_COUNT == 0)
                {
                    int newFullness = Fullness - SourceTemplate.Hungry;
                    if (newFullness <= 5)
                    {
                        PetItem.Fullness = 15;

                        await Recall(1);
                        await PetItem.PlayerInventory.Owner.dropMessage(6, "Your pet grew hungry! Treat it some pet food to keep it healthy!");
                    }
                    else
                    {
                        PetItem.Fullness = newFullness;
                        await PetItem.PlayerInventory.Owner.forceUpdateItem(PetItem);
                    }
                }
                else
                {
                    step++;
                }
                Next = now + Period;
            }
        }
    }
}
