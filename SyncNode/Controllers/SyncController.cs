using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyncNode.Services;

namespace SyncNode.Controllers
{
     [Route("[controller]")]
     [ApiController]
     public class SyncController : ControllerBase
     {
          private readonly SyncWorkJobService _workJobService;

          public SyncController(SyncWorkJobService workJobService)
          {
               _workJobService = workJobService;
          }


          [HttpPost]
          public IActionResult Sync(SyncEntity entity)
          {
               _workJobService.AddItem(entity);
                return Ok();
          }
     }
}
