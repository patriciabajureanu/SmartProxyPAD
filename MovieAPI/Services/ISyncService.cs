using Common.Models;

namespace MovieAPI.Services
{
     public interface ISyncService<T> where T : MongoDocument
     {
          HttpResponseMessage? Upsert(T record);

          HttpResponseMessage? Delete(T record);
     }
}
