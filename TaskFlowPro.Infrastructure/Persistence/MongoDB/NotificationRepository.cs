using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace TaskFlowPro.Infrastructure.Persistence.MongoDB;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(IOptions<MongoDbSettings> settings)
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Notification)))
        {
            BsonClassMap.RegisterClassMap<Notification>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(n => n.Id)
                  .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Notification>(
            settings.Value.NotificationsCollectionName
        );
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
    {
        return await _collection
            .Find(n => n.UserId == userId)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        return await _collection
            .Find(n => n.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Notification notification)
    {
        await _collection.InsertOneAsync(notification);
    }

    public async Task MarkAsReadAsync(string id)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(n => n.Id == id);
    }
}