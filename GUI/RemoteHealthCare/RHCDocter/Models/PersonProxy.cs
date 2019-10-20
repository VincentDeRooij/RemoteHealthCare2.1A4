using Newtonsoft.Json;
using RHCCore.Networking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCDocter.Models
{
    public class PersonProxy
    {
        [JsonProperty("person")]
        public Person Person { get; set; }
        [JsonProperty("is_online")]
        public bool IsOnline { get; set; }
        [JsonProperty("auth_key")]
        public string Key { get; set; }

        public PersonProxy(Person person, bool isOnline, string key)
        {
            this.Person = person;
            this.IsOnline = isOnline;
            this.Key = key;
        }
    }
}
