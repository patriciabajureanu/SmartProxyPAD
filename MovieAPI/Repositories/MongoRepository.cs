
using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieAPI.Settings;

namespace MovieAPI.Repositories
{
     public class MongoRepository<T> : IMongoRepository<T> where T :MongoDocument
     {
          private readonly IMongoDatabase _db;
          private readonly IMongoCollection<T> _collection;
          public MongoRepository(IMongoDbSettings dbSettings)
          {
               _db = new MongoClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName);

               string tableName = typeof(T).Name.ToLower();
               _collection = _db.GetCollection<T>(tableName);
          }

          public void DeleteRecord(Guid id)
          {
               _collection.DeleteOne(doc => doc.Id == id);

          }

          public List<T> GetAllRecords()
          {
               var records = _collection.Find(new BsonDocument()).ToList();
               return records;
          }

          public T GetRecordById(Guid id)
          {
               var record = _collection.Find(doc => doc.Id == id).FirstOrDefault();
               return record;
          }

          public T InsertRecord(T record)
          {
               _collection.InsertOne(record);
               return record;
          }

          public void UpsertRecord(T record)
          {
               _collection.ReplaceOne(doc => doc.Id == record.Id, record,
                    new ReplaceOptions() { IsUpsert = true });
          }
     }
}
