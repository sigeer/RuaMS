using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Dtos.Gachapon
{
    public class GachaponResponseDto
    {
        public int Id { get; set; }
        /// <summary>
        /// -1：全局
        /// </summary>
        public int NpcId { get; set; }
        public string NpcName { get; set; } = "";
    }

    public class GachaponDetailResponseDto : GachaponResponseDto
    {
        public List<GachaponItemResponseDto> Items { get; set; } = [];
        public List<GachaponSettingResponseDto> LevelSettings { get; set; } = [];
    }
}
