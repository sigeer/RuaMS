namespace Application.Templates.Mob
{
    public class MobTemplate : AbstractTemplate, ILinkTemplate<MobTemplate>
    {

        public int Level { get; set; }
        public int MaxHP { get; set; }
        public int MaxMP { get; set; }
        public int HpRecovery { get; set; }
        public int MpRecovery { get; set; }
        public int PAD { get; set; }
        public int PDD { get; set; }
        public int MAD { get; set; }
        public int MDD { get; set; }
        public int PDR { get; set; }
        public int MDR { get; set; }
        public int ACC { get; set; }
        public int EVA { get; set; }
        public int Exp { get; set; }

        public bool Boss { get; set; }
        public int[] Revive { get; set; }
        public int PublicReward { get; set; }
        public bool DamagedByMob { get; set; }
        public bool ExplosiveReward { get; set; }
        public bool UnDead { get; set; }
        public bool IsFirstAttack { get; set; }
        public MobDataSkillTemplate[] Skill { get; set; }
        public int Buff { get; set; } = -1;
        public int GetCP { get; set; }
        public bool RemoveOnMiss { get; set; }
        public int DropItemPeriod { get; set; }
        public int RemoveAfter { get; set; }
        public int HpTagColor { get; set; }
        public int HpTagBgColor { get; set; }
        public bool NoFlip { get; set; }


        public MobSelfDestruction? SelfDestruction { get; set; }
        public int CoolDamage { get; set; }
        public int CoolDamageProb { get; set; }

        public MobLosedItem[] LosedItems { get; set; }
        public string? ElementStr { get; set; }
        public MobBanTemplate? Ban { get; set; }

        public Dictionary<string, int> AnimateDelay { get; set; } = [];
        public int? Stand0OriginX { get; set; }
        public MobAttackTemplate[] AttackInfos { get; set; } = [];
        public int Link { get; set; }

        public MobTemplate(int templateId)
            : base(templateId)
        {
            Skill = Array.Empty<MobDataSkillTemplate>();
            Revive = Array.Empty<int>();
            LosedItems = Array.Empty<MobLosedItem>();
        }

        public sealed class MobDataSkillTemplate
        {
            public MobDataSkillTemplate(int index)
            {
                Index = index;
            }

            public int Index { get; set; }
            public int Action { get; set; }
            public int EffectAfter { get; set; }
            public int Level { get; set; }
            public int Skill { get; set; }
        }

        public sealed class MobLosedItem
        {
            public int Id { get; set; }
            public int Prop { get; set; }
            public int X { get; set; }
        }

        public sealed class MobBanTemplate
        {
            public string? Message { get; set; }

            [WZPath("info/ban/banMap/0/field")]
            public int Map { get; set; }
            [WZPath("info/ban/banMap/0/portal")]
            public string? PortalName { get; set; } = "sp";
        }

        public sealed class MobAttackTemplate
        {
            public MobAttackTemplate(int index)
            {
                Index = index;
                Animations = Array.Empty<MobAttackAnimationTemplate>();
            }

            public int Index { get; set; }
            public int AttackAfter { get; set; }
            public int ConMP { get; set; }
            public int PADamage { get; set; }
            public bool DeadlyAttack { get; set; }
            public int MpBurn { get; set; }
            public int Disease { get; set; }
            public int Level { get; set; }
            public MobAttackAnimationTemplate[] Animations { get; set; }
        }

        public sealed class MobAttackAnimationTemplate
        {
            public MobAttackAnimationTemplate(int index)
            {
                Index = index;
            }

            public int Index { get; set; }
            public int Delay { get; set; }
        }

        public void CloneLink(MobTemplate sourceTemplate)
        {
            //sourceTemplate.Ban = Ban;
            //sourceTemplate.LosedItems = LosedItems;
            // sourceTemplate.Skill = Skill;
            //sourceTemplate.Revive = Revive;
            sourceTemplate.AttackInfos = AttackInfos;

            //sourceTemplate.SelfDestructActionType = SelfDestructActionType;
            //sourceTemplate.SelfDestructHp = SelfDestructHp;
            //sourceTemplate.SelfDestructRemoveAfter = SelfDestructRemoveAfter;

            // sourceTemplate.AnimateDelay = AnimateDelay;
        }
    }
}
