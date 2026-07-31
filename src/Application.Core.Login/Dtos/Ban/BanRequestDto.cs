using Application.Shared.Login;
using System.ComponentModel.DataAnnotations;

namespace Application.Host.Models
{
    public class BanRequestDto
    {
        public int TargetAccountId { get; set; }
        [Range(1, 1000_000)]
        public int Hours { get; set; }
        public BanLevel BanLevel { get; set; }
        public BanReason Reason { get; set; }
        public string? ReasonDesc { get; set; }
    }
}
