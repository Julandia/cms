using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConventionManagementService.Model
{
    public class Convention : ICloneable
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public int TotalNumberOfParticipants { get; set; }

        public IEnumerable<Event> Events { get; set; }

        public UserInfo UserInfo { get; set; }

        public bool IsDeleted { get; set; }

        public object Clone()
        {
            Convention other = (Convention)this.MemberwiseClone();
            other.Events = this.Events.Select(ev => ev.Clone() as Event);
            return other;
        }

    }

    public class UserInfo
    {
        public string UserId { get; set; }

        public int NumberOfParticipants { get; set; }
    }
}
