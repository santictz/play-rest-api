using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Play.Catalog.Service.Repositories
{
    public class ItemsRepository : IItemsRepository
    {
        private const string collectionName = "items";
        private readonly IMongoCollection<Item> dbCollection;
        private readonly FilterDefinitionBuilder<Item> filterDefinitionBuilder = Builders<Item>.Filter;

        public ItemsRepository(IMongoDatabase database)
        {
            dbCollection = database.GetCollection<Item>(collectionName);
        }

        public async Task<IReadOnlyCollection<Item>> GetAllAsync()
        {
            return await dbCollection.Find(filterDefinitionBuilder.Empty).ToListAsync();
        }

        public async Task<Item> GetAsync(Guid id)
        {
            FilterDefinition<Item> filter = filterDefinitionBuilder.Eq(e => e.Id, id);
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Item entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            await dbCollection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Item entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            FilterDefinition<Item> filter = filterDefinitionBuilder.Eq(e => e.Id, entity.Id);
            await dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            FilterDefinition<Item> filter = filterDefinitionBuilder.Eq(e => e.Id, id);
            await dbCollection.DeleteOneAsync(filter);
        }
    }
}
