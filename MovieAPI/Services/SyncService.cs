using System.Text.Json;
using Common.Models;
using Common.Utilities;
using MovieAPI.Settings;

namespace MovieAPI.Services
{
     public class SyncService<T> : ISyncService<T> where T : MongoDocument
     {
          private readonly ISyncServiceSettings _settings;
          private readonly ILogger<SyncService<T>> _logger;
          private readonly string _origin;

          public SyncService(ISyncServiceSettings settings, IHttpContextAccessor httpContext, ILogger<SyncService<T>> logger)
          {
               _settings = settings;
               _logger = logger;
               _origin = httpContext.HttpContext.Request.Host.ToString();
          }
          public HttpResponseMessage? Delete(T record)
          {
               return Send(record, _settings.DeleteHttpMethod);
          }

          public HttpResponseMessage? Upsert(T record)
          {
               return Send(record, _settings.UpsertHttpMethod);
          }

          // scrierea în baza locală e deja făcută; dacă SyncNode e oprit doar logăm, clientul nu primește 500
          private HttpResponseMessage? Send(T record, string syncType)
          {
               var json = ToSyncEntityJson(record, syncType);
               try
               {
                    var response = HttpClientUtility.SendJson(json, _settings.Host, "POST");
                    if (!response.IsSuccessStatusCode)
                    {
                         _logger.LogWarning("SyncNode returned {Status} for {Id}", response.StatusCode, record.Id);
                    }
                    return response;
               }
               catch (Exception e)
               {
                    _logger.LogWarning("SyncNode unavailable, {Id} not replicated: {Error}", record.Id, e.Message);
                    return null;
               }
          }

          private string ToSyncEntityJson(T record, string syncType)
          {
               var objectType = typeof(T);

               var syncEntity = new SyncEntity()
               {
                    JsonData = JsonSerializer.Serialize(record),
                    SyncType = syncType,
                    ObjectType = objectType.Name, //api/sync/movie
                    Id = record.Id,
                    LastChangeAt = record.LastChangedAt,
                    Origin = _origin
               };

               var json = JsonSerializer.Serialize(syncEntity);
               return json;

          }

     }
}
