using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;
using System.Threading.Tasks;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repository;

        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            //Create item on each message consumed
            var message = context.Message;
            var item = await repository.GetAsync(message.ItemId);

            //If the item already exists in the DB skip
            if (item is null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };
                await repository.CreateAsync(item);
                return;
            }

            item.Name = message.Name;
            item.Description = message.Description;

            await repository.UpdateAsync(item);

            return;
        }
    }
}
