using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConventionManagementService.Model
{
    public enum EventType
    {
        Event = 0,
        Talk,
        Venue
    }

    public class Event : ICloneable
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public EventType Type { get; set; }

        public string Title { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public int TotalNumberOfParticipants { get; set; }

        public UserInfo UserInfo { get; set; }

        public object Clone()
        {
            Event other = (Event)this.MemberwiseClone();
            return other;
        }
    }
}
