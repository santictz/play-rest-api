using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
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
        private readonly IItemsRepository repository;

        public ItemsController(IItemsRepository repository)
        {
            this.repository = repository;
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
            return NoContent();
        }
    }
}
