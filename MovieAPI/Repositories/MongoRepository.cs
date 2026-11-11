
using Common.Models;
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

          public List<T> GetAllRecords(int? offset = null, int? limit = null)
          {
               return _collection.Find(doc => !doc.IsDeleted).Skip(offset).Limit(limit).ToList();
          }

          public T GetRecordById(Guid id)
          {
               return _collection.Find(doc => doc.Id == id && !doc.IsDeleted).FirstOrDefault();
          }

          // folosit la sync: vede și înregistrările șterse (tombstone)
          public T GetRecordByIdRaw(Guid id)
          {
               return _collection.Find(doc => doc.Id == id).FirstOrDefault();
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
