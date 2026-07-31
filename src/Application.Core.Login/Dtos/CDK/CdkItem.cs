using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Core.Login.Dtos.CDK
{
    public class RewardItemResponseDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
    }
}
