using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleCosmosDB.Model
{
    public class ScaleCosmos
    {
        [JsonProperty("CollectionName")]
        public string CollectionName { get; set; }

        [JsonProperty("Database")]
        public string Database { get; set; }

        [JsonProperty("ScaleUp")]
        public bool ScaleUp { get; set; }
    }
}
