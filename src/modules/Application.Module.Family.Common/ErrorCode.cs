namespace Application.Module.Family.Common
{

    /// <summary>
    /// <para>70: This character is already a Junior of another character.</para>
    /// <para>71: The Junior you wish to add\r\nmust be at a lower rank.</para>
    /// <para>72: The gap between you and your\r\njunior must be within 20 levels.</para>
    /// <para>73: Another character has requested to add this character.Please try again later.</para>
    /// <para>74: Another character has requested a summon.Please try again later.</para>
    /// <para>75: The summons has failed. Your current location or state does not allow a summons.</para>
    /// <para>76: The family cannot extend more than 1000 generations from above and below.</para>
    /// <para>77: The Junior you wish to add\r\nmust be over Level 10.</para>
    /// <para>78: You cannot add a Junior that has requested to change worlds.</para>
    /// <para>79: You cannot add a Junior \r\nsince you've requested to change worlds.</para>
    /// <para>80: Separation is not possible due to insufficient Mesos.\r\nYou will need %d Mesos to\r\nseparate with a Senior.</para>
    /// <para>81: Separation is not possible due to insufficient Mesos.\r\nYou will need %d Mesos to\r\nseparate with a Junior.</para>
    /// <para>82: The Entitlement does not apply because your level does not match the corresponding area.</para>
    /// </summary>
    public class ErrorCode
    {
        public const int NotLeader = 1;
        public const int AlreadyInSameFamily = 2;
        public const int OverMaxGenerations = 76;
    }
}
