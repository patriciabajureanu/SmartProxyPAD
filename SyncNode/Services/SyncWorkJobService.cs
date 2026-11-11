using System.Collections.Concurrent;
using Common.Models;
using Common.Utilities;
using SyncNode.Settings;

namespace SyncNode.Services
{
     public class SyncWorkJobService : IHostedService
     {
          private const int MaxAttempts = 10;
          private const int IntervalMs = 5000;

          private readonly ConcurrentDictionary<Guid, SyncEntity> documents =
               new ConcurrentDictionary<Guid, SyncEntity>();
          private readonly IMovieAPISettings _settings;
          private readonly ILogger<SyncWorkJobService> _logger;
          private Timer _timer;

          public SyncWorkJobService(IMovieAPISettings settings, ILogger<SyncWorkJobService> logger)
          {
               _settings = settings;
               _logger = logger;
          }

          // atomic: păstrează versiunea cu LastChangeAt mai mare
          public void AddItem(SyncEntity entity)
          {
               documents.AddOrUpdate(entity.Id, entity,
                    (_, existing) => entity.LastChangeAt >= existing.LastChangeAt ? entity : existing);
          }

          public Task StartAsync(CancellationToken cancellationToken)
          {
               // one-shot, re-armat la finalul DoSendWork, ca rulările să nu se suprapună
               _timer = new Timer(DoSendWork, null, IntervalMs, Timeout.Infinite);
               return Task.CompletedTask;
          }

          public Task StopAsync(CancellationToken cancellationToken)
          {
               _timer?.Change(Timeout.Infinite, 0);

               return Task.CompletedTask;
          }

          private void DoSendWork(object state)
          {
               try
               {
                    foreach (var key in documents.Keys)
                    {
                         if (documents.TryRemove(key, out var entity))
                         {
                              SendToReceivers(entity);
                         }
                    }
               }
               finally
               {
                    _timer.Change(IntervalMs, Timeout.Infinite);
               }
          }

          private void SendToReceivers(SyncEntity entity)
          {
               var failed = false;
               var receivers = _settings.Hosts.Where(x => !x.Contains(entity.Origin));
               foreach (var receiver in receivers)
               {
                    var url = $"{receiver}/sync/{entity.ObjectType}";
                    try
                    {
                         var result = HttpClientUtility.SendJson(entity.JsonData, url, entity.SyncType);
                         if (!result.IsSuccessStatusCode)
                         {
                              failed = true;
                              _logger.LogWarning("{Type} {Id} -> {Url}: {Status}", entity.SyncType, entity.Id, url, result.StatusCode);
                         }
                    }
                    catch (Exception e)
                    {
                         failed = true;
                         _logger.LogWarning("{Type} {Id} -> {Url}: {Error}", entity.SyncType, entity.Id, url, e.Message);
                    }
               }

               if (!failed)
               {
                    _logger.LogInformation("{Type} {Id} replicated from {Origin}", entity.SyncType, entity.Id, entity.Origin);
                    return;
               }

               entity.Attempts++;
               if (entity.Attempts >= MaxAttempts)
               {
                    _logger.LogError("{Type} {Id} dropped after {Attempts} attempts", entity.SyncType, entity.Id, entity.Attempts);
                    return;
               }
               // înapoi în coadă; dacă între timp a venit o versiune mai nouă, rămâne aceea
               documents.AddOrUpdate(entity.Id, entity,
                    (_, existing) => existing.LastChangeAt >= entity.LastChangeAt ? existing : entity);
          }
     }
}
