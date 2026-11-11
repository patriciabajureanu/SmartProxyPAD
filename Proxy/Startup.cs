using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;

namespace Proxy
{
     public class Startup
     {
          public void ConfigureServices(IServiceCollection services)
          {
               services.AddOcelot().AddCacheManager(x =>
               {
                    x.WithDictionaryHandle();
               });
          }
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
          {
               if (env.IsDevelopment())
               {
                    app.UseDeveloperExceptionPage();
               }
               // clientul web (wwwroot/index.html) pe "/", restul cererilor merg prin Ocelot
               app.UseDefaultFiles();
               app.UseStaticFiles();

               app.UseOcelot().Wait();
          }
     }
}
