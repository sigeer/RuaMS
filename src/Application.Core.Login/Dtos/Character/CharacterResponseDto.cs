using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Dtos.Character
{
    public class CharacterResponseDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Job { get; set; }
        public string JobName { get; set; }
        public bool IsOnline { get; set; }
    }
}
