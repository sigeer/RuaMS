using Application.Core.Channel.Commands;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Skills;
using Application.Scripting.JS;
using client.status;
using Google.Protobuf.WellKnownTypes;
using server;
using System.Collections.Generic;
using tools;
using static Application.Core.Channel.Internal.Handlers.PlayerFieldHandlers;

namespace Application.Core.Game.Maps.Mists
{
    public class PlayerMist : Mist
    {
        public int OwnerId { get; }
        public StatEffect Source { get; }

        public PlayerMist(Rectangle mistPosition, Player owner, StatEffect source) : base(owner.getMap(), mistPosition, source.getDuration())
        {
            this.OwnerId = owner.Id;
            this.Source = source;
            _isMobMist = false;
            _isRecoveryMist = false;
            _isPoisonMist = false;
            switch (source.getSourceId())
            {
                case Evan.RECOVERY_AURA:
                    _isRecoveryMist = true;
                    break;

                case Shadower.SMOKE_SCREEN: // Smoke Screen
                    _isPoisonMist = false;
                    break;

                case FPMage.POISON_MIST: // FP mist
                case BlazeWizard.FLAME_GEAR: // Flame Gear
                case NightWalker.POISON_BOMB: // Poison Bomb
                    _isPoisonMist = true;
                    break;
            }
        }

        public Skill getSourceSkill()
        {
            return SkillFactory.getSkill(Source.getSourceId());
        }

        public override Packet makeSpawnData()
        {
            return PacketCreator.spawnMist(getObjectId(), OwnerId, Source.getSourceId(), Source.SkillLevel, this);
        }

        public override Packet makeFakeSpawnData(int level)
        {
            return PacketCreator.spawnMist(getObjectId(), OwnerId, Source.getSourceId(), level, this);
        }

        public bool makeChanceResult()
        {
            return Source.makeChanceResult();
        }

        public void ApplyMistEffect()
        {
            if (_isPoisonMist)
            {
                var owner = getMap().getCharacterById(OwnerId);
                if (owner == null)
                {
                    return;
                }

                List<IMapObject> affectedMonsters = getMap().getMapObjectsInBox(getBox(), Collections.singletonList(MapObjectType.MONSTER));
                foreach (IMapObject mo in affectedMonsters)
                {
                    if (makeChanceResult())
                    {
                        MonsterStatusEffect poisonEffect = new MonsterStatusEffect(Collections.singletonMap(MonsterStatus.POISON, 1), getSourceSkill());
                        ((Monster)mo).applyStatus(owner, poisonEffect, true, Source.getDuration());
                    }
                }
            }

            else if (_isRecoveryMist)
            {
                List<IMapObject> players = getMap().getMapObjectsInBox(getBox(), Collections.singletonList(MapObjectType.PLAYER));
                foreach (IMapObject mo in players)
                {
                    if (makeChanceResult())
                    {
                        Player chr = (Player)mo;
                        if (OwnerId == chr.getId() || (chr.getParty()?.containsMembers(OwnerId) ?? false))
                        {
                            chr.ChangeMP(Source.getX() * chr.MP / 100);
                        }
                    }
                }
            }
        }

        public ScheduledFuture? EffectScheduler { get; private set; }
        public override void Enter(IMap map, Action<Player> chrAction)
        {
            base.Enter(map, chrAction);

            EffectScheduler = map.ChannelServer.Node.TimerManager.register(new MapTaskBase(map, $"PlayerMistEffectTask_{GetHashCode()}", () =>
            {
                map.ChannelServer.Post(new PlayerMistEffectCommand(this));
            }), 2000, 2500);
        }

        public override void Leave(Action<Player> chrAction)
        {
            EffectScheduler?.cancel(false);

            base.Leave(chrAction);
        }
    }
}
