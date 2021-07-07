using Microsoft.AspNetCore.Mvc;
using Play.Common;
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
        private readonly IRepository<InventoryItem> inventoryRepository;
        private readonly IRepository<CatalogItem> catalogRepository;

        public ItemsController(IRepository<InventoryItem> inventoryRepository, IRepository<CatalogItem> catalogRepository)
        {
            this.inventoryRepository = inventoryRepository;
            this.catalogRepository = catalogRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();

            var inventoryItemsEntities = await inventoryRepository.GetAllAsync(item => item.Id == userId);
            var itemIds = inventoryItemsEntities.Select(item => item.CatalogItemId);
            var catalogItemEntities = await catalogRepository.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemsDtos = inventoryItemsEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.SingleOrDefault(catalog => catalog.Id == inventoryItem.Id);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemsDtos);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await inventoryRepository.GetAsync(item => item.UserId == grantItemsDto.UserId
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

                await inventoryRepository.CreateAsync(inventoryItem);
                return Ok();
            }

            inventoryItem.Quantity += grantItemsDto.Quantity;
            await inventoryRepository.UpdateAsync(inventoryItem);
            return Ok();
        }
    }
}
