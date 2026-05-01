namespace Application.Shared.Constants
{
    //000. NPC左边
    //001. NPC左边、无结束对话

    //010. 玩家右边
    //011. 玩家右边、无结束对话

    //100. NPC右边 
    //101. NPC右边、无结束对话
    //110. 玩家右边
    //111. 玩家右边、无结束对话
    // (x & 6) > 0

    //1000. NPC左边、脸朝向
    //1001. NPC左边、脸朝向、无结束对话

    //1010. 玩家右边
    //1011. 玩家右边、无结束对话
    //1100. NPC右边、脸朝向
    //1101. NPC右边、脸朝向、无结束对话
    public enum NpcTalkSpeaker : byte
    {
        /// <summary>
        /// 对Player无效
        /// </summary>
        Left = 0,
        WithoutEnd = 1 << 0,
        /// <summary>
        /// 玩家，固定右边
        /// </summary>
        Player = 1 << 1,
        /// <summary>
        /// Npc在右边，另传NPCId
        /// <para>(x &#38; 4) &gt; 0</para>
        /// </summary>
        ExtraNpc = 1 << 2,
        Face = 1 << 3

    }
}
