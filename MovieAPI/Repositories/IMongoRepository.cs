using Common.Models;

namespace MovieAPI.Repositories
{
      public interface IMongoRepository<T> where T : MongoDocument
     {
          List<T> GetAllRecords(int? offset = null, int? limit = null);
          public T InsertRecord(T record);
          T GetRecordById(Guid id);
          void UpsertRecord(T record);

     }
}
