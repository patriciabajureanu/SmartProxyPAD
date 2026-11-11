using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Repositories;
using MovieAPI.Services;

namespace MovieAPI.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class MovieController : ControllerBase
     {
          private readonly IMongoRepository<Movie> _movieRepository;
          private readonly ISyncService<Movie> _movieSyncService;
          public MovieController(IMongoRepository<Movie> movieRepository,
               ISyncService<Movie> movieSyncService)
          {
               _movieRepository = movieRepository;
               _movieSyncService = movieSyncService;

          }
          [HttpGet]
          public ActionResult<List<Movie>> GetAllMovies(int? offset, int? limit)
          {
               if (offset < 0 || limit < 0)
               {
                    return BadRequest("offset and limit must be >= 0");
               }
               return _movieRepository.GetAllRecords(offset, limit);
          }

          [HttpGet("{id}")]
          public ActionResult<Movie> GetMovieById(Guid id)
          {
               var result = _movieRepository.GetRecordById(id);
               if (result == null)
               {
                    return NotFound();
               }
               return result;
          }
          [HttpPost]
          public IActionResult Create(Movie movie)
          {
               movie.Id = Guid.Empty; // id-ul îl generează serverul; un id existent ar da duplicate key (500)
               movie.LastChangedAt = DateTime.UtcNow;
               movie.IsDeleted = false;

               var result = _movieRepository.InsertRecord(movie);

               _movieSyncService.Upsert(movie);
               return Ok(result);
          }
          [HttpPut]
          public IActionResult Upsert(Movie movie)
          {
               if (movie.Id == Guid.Empty)
               {
                    return BadRequest("Empty Id");
               }
               movie.LastChangedAt = DateTime.UtcNow;
               movie.IsDeleted = false;
               _movieRepository.UpsertRecord(movie);

               _movieSyncService.Upsert(movie);

               return Ok(movie);

          }
          // endpoint-uri interne, apelate doar de SyncNode; proxy-ul nu le expune
          [HttpPut("/api/sync/movie")]
          public IActionResult UpsertSync(Movie movie)
          {
               _movieRepository.UpsertRecord(movie); // se aplică doar dacă e mai nou (delete vs update)
               return Ok();
          }
          [HttpDelete("/api/sync/movie")]
          public IActionResult DeleteSync(Movie movie)
          {
               movie.IsDeleted = true;
               _movieRepository.UpsertRecord(movie);
               return Ok();
          }

          [HttpDelete ("{id}")]
          public IActionResult Delete(Guid id)
          {
               var movie = _movieRepository.GetRecordById(id);
               if (movie == null)
               {
                    return NotFound();
               }
               movie.IsDeleted = true;
               movie.LastChangedAt = DateTime.UtcNow;
               _movieRepository.UpsertRecord(movie);

               _movieSyncService.Delete(movie);
               return Ok("Deleted " + id);
          }

     }
}
