using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Models
{
     public abstract class MongoDocument
     {
          [BsonId]
          [BsonGuidRepresentation(GuidRepresentation.Standard)]
          public Guid Id { get; set; }
          public DateTime LastChangedAt { get; set; }
          // tombstone: ștergerea se replică și nu poate fi anulată de un update mai vechi
          public bool IsDeleted { get; set; }
     }
}
