using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieAPI.Repositories;
using MovieAPI.Services;
using MovieAPI.Settings;

namespace MovieAPI
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
               services.AddHttpContextAccessor();
               services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDbSettings"));
               services.Configure<SyncServiceSettings>(Configuration.GetSection("SyncServiceSettings"));

               services.AddSingleton<IMongoDbSettings>(provider =>
                    provider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

               services.AddSingleton<ISyncServiceSettings>(provider =>
                    provider.GetRequiredService<IOptions<SyncServiceSettings>>().Value);

               services.AddScoped<IMongoRepository<Movie>, MongoRepository<Movie>>();
               services.AddScoped<ISyncService<Movie>, SyncService<Movie>>();

               // clientul alege JSON sau XML prin header-ul Accept
               services.AddControllers().AddXmlSerializerFormatters();
          }

          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {
               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
               }
               // arată ce nod a servit cererea (round robin vizibil prin proxy)
               app.Use((context, next) =>
               {
                    context.Response.Headers["X-Served-By"] = context.Connection.LocalPort.ToString();
                    return next();
               });
               app.UseRouting();
               app.UseAuthorization();
               app.UseEndpoints(endpoints =>
               {
                    endpoints.MapControllers();
               });
          }
     }
}
