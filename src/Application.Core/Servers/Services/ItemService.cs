using Application.Core.Game.Items;
using Application.Core.ServerTransports;
using Application.Shared.Characters;
using AutoMapper;
using client.inventory;
using Microsoft.Extensions.Logging;
using server;

namespace Application.Core.Servers.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;

        public ItemService(IMapper mapper, IChannelServerTransport transport, ILogger<ItemService> logger)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
        }

        private PetDto CreatePet(string petName, int level, int tameness, int fullness)
        {
            return _transport.CreatePet(petName, level, tameness, fullness);
        }

        public PetDto CreatePet(int itemId)
        {
            return CreatePet(ItemInformationProvider.getInstance().getName(itemId), 1, 0, 100);
        }

        //public Item CreatePet(int itemId, short position)
        //{
        //    return CreateItem(itemId, position, 1, CreatePet(itemId));
        //}

        //public Item CreateItem(int itemId, short position, short quantity, PetDto? pet)
        //{
        //    if (pet == null)
        //        return new Item(itemId, position, quantity);

        //    return new Item(itemId, position, 1, GetPetByDto(itemId, position, pet));
        //}

        public Pet? GetPetByDto(int itemid, short position, PetDto? pet)
        {
            if (pet == null)
                return null;

            Pet ret = new Pet(itemid, position, pet.Petid);
            ret.Summoned = pet.Summoned;
            ret.PetAttribute = pet.Flag;
            ret.Fullness = pet.Fullness;
            ret.Tameness = pet.Closeness;
            ret.Level = (byte)pet.Level;
            ret.Name = pet.Name ?? "";
            return ret;
        }
    }
}
