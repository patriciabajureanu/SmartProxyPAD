using Common.Models;

namespace MovieAPI.Repositories
{
      public interface IMongoRepository<T> where T : MongoDocument
     {
          List<T> GetAllRecords();
          public T InsertRecord(T record);
          T GetRecordById(Guid id);
          void UpsertRecord(T record);
          void DeleteRecord(Guid id);

     }
}
