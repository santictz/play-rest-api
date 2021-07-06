using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Inventory.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> itemsRepository;
        private readonly CatalogClient catalogClient;

        public ItemsController(IRepository<InventoryItem> repository, CatalogClient catalogClient)
        {
            this.itemsRepository = repository;
            this.catalogClient = catalogClient;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync([FromBody] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();

            var catalogItems = await catalogClient.GetCatalogItemsAsync();
            var inventoryItemsEntities = await itemsRepository.GetAllAsync(item => item.Id == userId);

            var inventoryItemsDtos = inventoryItemsEntities.Select(item =>
            {
                var catalogItem = catalogItems.SingleOrDefault(catalog => catalog.Id == item.Id);
                return item.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemsDtos);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await itemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId
            && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await itemsRepository.CreateAsync(inventoryItem);
                return Ok();
            }

            inventoryItem.Quantity += grantItemsDto.Quantity;
            await itemsRepository.UpdateAsync(inventoryItem);
            return Ok();
        }
    }
}
