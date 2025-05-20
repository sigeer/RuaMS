using Application.Shared.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class StorageDto
    {

        public StorageDto() { }

        public StorageDto(int accountid)
        {
            Accountid = accountid;
            Slots = 4;
            Meso = 0;
            Items = [];
        }

        public int Id { get; set; }
        public int Accountid { get; set; }


        public byte Slots { get; set; }

        public int Meso { get; set; }
        public ItemDto[] Items { get; set; }
    }
}
