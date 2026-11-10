using Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncNode.Services;
using SyncNode.Settings;

namespace SyncNode
{
     public class Startup
     {
          public Startup(IConfiguration configuration)
          {
               Configuration = configuration;
          }

          public IConfiguration Configuration { get; }

          public void ConfigureServices(IServiceCollection services)
          {
               services.Configure<MovieAPISettings>(Configuration.GetSection("MovieAPISettings"));

               services.AddSingleton<IMovieAPISettings>(provider =>
                    provider.GetRequiredService<IOptions<MovieAPISettings>>().Value);

               services.AddSingleton<SyncWorkJobService>();

               services.AddHostedService(provider => provider.GetService<SyncWorkJobService>());

               services.AddControllers();
          }
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {
               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
               }
               app.UseRouting();
               app.UseAuthorization();
               app.UseEndpoints(endpoints =>
               {
                    endpoints.MapControllers();
               });
          }

     }
}