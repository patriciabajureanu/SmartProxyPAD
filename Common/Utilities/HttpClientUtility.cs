using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
     public class HttpClientUtility
     {
          private static readonly HttpClient _client = new HttpClient();

          public static HttpResponseMessage SendJson(string json, string url, string method)
          {
               var httpMethod = new HttpMethod(method.ToUpper());
               var content = new StringContent(json, Encoding.UTF8, "application/json");

               var request = new HttpRequestMessage(httpMethod, url)
               {
                    Content = content
               };

               var task = _client.SendAsync(request);
               task.Wait();

               return task.Result;
          }
     }
}
