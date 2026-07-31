using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Dtos.Account
{

    public class AccountResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GMLevel { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public AccountBanPreviewDto? BanInfo { get; set; }
    }

    public class AccountBanPreviewDto
    {
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
    }
}
