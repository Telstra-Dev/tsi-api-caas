using System;

namespace WCA.Consumer.Api.Models
{
    public class HealthStatusModel : IEquatable<HealthStatusModel>
    {
        public HealthStatusCode Code { get; set; }
        public string Reason { get; set; }
        public string Action { get; set; }

        public bool Equals(HealthStatusModel other)
        {
            if (other == null) {
                return false;
            }

            return this.Code == other.Code &&
                this.Reason == other.Reason &&
                this.Action == other.Action;
        }
    }
}