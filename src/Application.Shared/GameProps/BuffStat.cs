namespace Application.Shared.GameProps
{
    /// <summary>
    /// 角色 TemporaryStat（客户端 SecondaryStat）状态。
    ///
    /// <para>
    /// <b>枚举值 = 客户端 128 位掩码里的位号</b>，与测试工程里的同名对照表
    /// <c>test/ServiceTest/Infrastructure/TemporaryStatType.cs</c> 一一对应。
    /// 封包层直接用 <c>(int)stat</c> 定位字段、生成掩码，不需要任何换算：
    /// </para>
    /// <list type="bullet">
    /// <item>位 0..31 → secondmask 的高 dword，位 32..63 → secondmask 的低 dword；</item>
    /// <item>位 64..95 → firstmask 的高 dword，位 96..127 → firstmask 的低 dword。</item>
    /// </list>
    /// <para>
    /// （客户端的 UINT128 是按 dword 逆序存放的，掩码与字段顺序见 <c>BuffPackets</c> 里的说明。）
    /// </para>
    /// <para>
    /// 别名：同一个位可以有多个名字（<c>COMBO</c>/<c>SUMMON</c>、<c>SHADOW_CLAW</c>/<c>FISHABLE</c>、
    /// <c>COUPON_EXP*</c>/<c>ITEM_UP_BY_ITEM</c>/<c>RESPECT_*</c>/<c>DEFENSE_*</c>）。
    /// 它们值相同，写包时同一位只写一份；<see cref="object.ToString"/> 返回声明在前面的那个名字，
    /// 用 <see cref="BuffStatUtils.TryParse"/> 两个名字都能还原。
    /// </para>
    /// <para>
    /// 疾病（原 <c>Disease</c> 类型）也已并入这里，元数据（mob skill 类型等）见 <see cref="DiseaseInfo"/>。
    /// </para>
    /// </summary>
    public enum BuffStat
    {
        // —— 常规 buff / 属性（位 0..31）——
        WATK = 0,
        WDEF = 1,
        MATK = 2,
        MDEF = 3,
        ACC = 4,
        AVOID = 5,
        HANDS = 6,
        SPEED = 7,
        JUMP = 8,
        MAGIC_GUARD = 9,
        DARKSIGHT = 10,
        BOOSTER = 11,
        POWERGUARD = 12,
        HYPERBODYHP = 13,
        HYPERBODYMP = 14,
        INVINCIBLE = 15,
        SOULARROW = 16,
        STUN = 17,
        POISON = 18,
        SEAL = 19,
        DARKNESS = 20,
        COMBO = 21,
        SUMMON = 21,
        WK_CHARGE = 22,
        DRAGONBLOOD = 23,
        HOLY_SYMBOL = 24,
        MESOUP = 25,
        SHADOWPARTNER = 26,
        PICKPOCKET = 27,
        MESOGUARD = 28,
        THAW = 29,
        WEAKEN = 30,
        CURSE = 31,

        // —— 第二组（位 32..63）——
        SLOW = 32,
        MORPH = 33,
        RECOVERY = 34,
        MAPLE_WARRIOR = 35,
        STANCE = 36,
        SHARP_EYES = 37,
        MANA_REFLECTION = 38,
        SEDUCE = 39,
        SHADOW_CLAW = 40,
        FISHABLE = 40,
        INFINITY = 41,
        HOLY_SHIELD = 42,
        HAMSTRING = 43,
        BLIND = 44,
        CONCENTRATE = 45,
        PUPPET = 46,
        ECHO_OF_HERO = 47,
        MESO_UP_BY_ITEM = 48,
        GHOST_MORPH = 49,
        AURA = 50,
        CONFUSE = 51,
        COUPON_EXP1 = 52,
        ITEM_UP_BY_ITEM = 52,
        COUPON_EXP2 = 53,
        RESPECT_PIMMUNE = 53,
        COUPON_EXP3 = 54,
        COUPON_EXP4 = 54,
        RESPECT_MIMMUNE = 54,
        COUPON_DRP1 = 55,
        DEFENSE_ATT = 55,
        COUPON_DRP2 = 56,
        COUPON_DRP3 = 56,
        DEFENSE_STATE = 56,
        HPREC = 57,
        MPREC = 58,
        BERSERK_FURY = 59,
        DIVINE_BODY = 60,
        SPARK = 61,
        MAP_CHAIR = 62,
        FINALATTACK = 63,

        // —— firstmask 组（位 64..81）——
        WINDBREAKERFINAL = 64,
        ELEMENTAL_RESET = 65,
        WIND_WALK = 66,
        EVENTRATE = 67,
        ARAN_COMBO = 68,
        COMBO_DRAIN = 69,
        COMBO_BARRIER = 70,
        BODY_PRESSURE = 71,
        SMART_KNOCKBACK = 72,
        BERSERK = 73,
        EXP_BUFF = 74,
        StopPotion = 75,
        StopMotion = 76,
        FEAR = 77,
        EVANSLOW = 78,
        MAGIC_SHIELD = 79,
        MAGIC_RESISTANCE = 80,
        SOULSTONE = 81,

        // —— 特殊字段（位 82..88，客户端各有独立解码对象）——
        ENERGY_CHARGE = 82,
        DASH2 = 83,
        DASH = 84,
        MONSTER_RIDING = 85,
        SPEED_INFUSION = 86,
        HOMING_BEACON = 87,
        NOTDAMAGED = 88,

        /// <summary>丧尸状态：服务端逻辑用，位 89 超出客户端包字段范围，客户端不会显示。</summary>
        ZOMBIFY = 89,
    }
}
