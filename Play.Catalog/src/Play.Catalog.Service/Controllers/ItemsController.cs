using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Entities;
using Play.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> repository;
        private readonly IPublishEndpoint publishEndpoint;

        public ItemsController(IRepository<Item> repository, IPublishEndpoint publishEndpoint)
        {
            this.repository = repository;
            this.publishEndpoint = publishEndpoint;
        }

        // GET: api/<ItemsController>
        [HttpGet]
        public async Task<IEnumerable<ItemDto>> Get()
        {
            var items = (await repository.GetAllAsync()).Select(i => i.AsDto());
            return items;
        }

        // GET api/<ItemsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(Guid id)
        {
            var item = await repository.GetAsync(id);

            return item is null ?
                NotFound() : item.AsDto();
        }

        // POST api/<ItemsController>
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Create([FromBody] CreatedItemDto value)
        {
            var item = new Item
            {
                Name = value.Name,
                CreatedDate = DateTimeOffset.UtcNow,
                Description = value.Description,
                Price = value.Price
            };

            await repository.CreateAsync(item);

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetById), new { item.Id }, item);
        }

        // PUT api/<ItemsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemDto value)
        {
            var existingItem = await repository.GetAsync(id);

            if (existingItem is null)
                return NotFound();

            existingItem.Name = value.Name;
            existingItem.Price = value.Price;
            existingItem.Description = value.Description;

            await repository.UpdateAsync(existingItem);

            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id,
                                                                 existingItem.Name,
                                                                 existingItem.Description));

            return NoContent();
        }

        // DELETE api/<ItemsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingItem = await repository.GetAsync(id);

            if (existingItem is null)
                return NotFound();

            await repository.RemoveAsync(id);

            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}
