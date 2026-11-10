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
               app.UseRouting();

               app.UseEndpoints(endpoints =>
               {
                    endpoints.MapGet("/", async context =>
                    {
                         await context.Response.WriteAsync("Smart Proxy");

                    });
               });
               app.UseOcelot().Wait();
          }
     }
}
