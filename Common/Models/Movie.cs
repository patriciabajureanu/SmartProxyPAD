namespace Common.Models
{
     public class Movie : MongoDocument
     {
          public string Name { get; set; }
          public List<string> Actors { get; set; }
          public decimal? Budget { get; set; }
          public string? Description { get; set; }

     }
}
