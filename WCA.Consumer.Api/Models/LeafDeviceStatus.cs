using System;

namespace WCA.Consumer.Api.Models
{
    public class LeafDeviceStatus
    {
        public int LeafDeviceId { get; set; }
        public int EdgeDeviceId { get; set; }
        public string LastHealthReadingTimestamp { get; set; }
        public long LastTelemetryReadingTimestamp { get; set; }
        public bool RequiresConfiguration { get; set; }
        public bool EdgeDeviceIsOnline { get; set; }
        public int DeviceRecentlyOnlineMaxMinutes { get; set; }
        public int DeviceRecentlySentTelemetryMaxMinutes { get; set; }

        public HealthStatusModel GetHealthStatus()
        {
            // var health = new HealthStatusModel();
            var health = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null,
            };

            // Priority 1: Check edge device ("gateway") online status
            // TODO: validate whether this health is real (likely needs integration with IOT-SC health APIs, if any)
            if (!EdgeDeviceIsOnline)
            {
                health.Code = HealthStatusCode.RED;
                health.Reason = "Camera offline";
                health.Action = "Contact support";
            }

            // Priority 2: Check configuration status
            else if (RequiresConfiguration)
            {
                health.Code = HealthStatusCode.AMBER;
                health.Reason = "Configure camera";
                health.Action = "Config in camera menu";
            }

            // Priority 3: Check online status
            else if (!CheckLeafDeviceRecentlyOnline(LastHealthReadingTimestamp, DeviceRecentlyOnlineMaxMinutes))
            {
                health.Code = HealthStatusCode.RED;
                health.Reason = "Camera offline";
                health.Action = "Contact support";
            }

            // Priority 4: Check data online status
            else if (!CheckLeafDeviceRecentlySentTelemetry(LastTelemetryReadingTimestamp, DeviceRecentlySentTelemetryMaxMinutes))
            {
                health.Code = HealthStatusCode.AMBER;
                health.Reason = "Data offline";
                health.Action = "Contact support";
            }

            // Priority 5: Assume asset is online.
            return health;
        }

        // Return true if `NOW() <= (lastTimestamp + DeviceRecentlyOnlineMaxMinutes)` i.e. if no heartbeat for `DeviceRecentlyOnlineMaxMinutes` minutes or more, camera is considered offline.
        public static bool CheckLeafDeviceRecentlyOnline(string lastActiveTime, int maxTimeMinutes)
        {
            DateTime dateTimeFromDB;
            if (DateTime.TryParse(lastActiveTime, out dateTimeFromDB))
            {
                if (DateTime.UtcNow < dateTimeFromDB.AddMinutes(maxTimeMinutes))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckLeafDeviceRecentlySentTelemetry(long? lastTelemetryTime, int maxTimeMinutes)
        {
            if (lastTelemetryTime != null)
            {
                DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)lastTelemetryTime);
                TimeSpan span = ((DateTimeOffset)DateTime.UtcNow).Subtract(timestamp);
                if (span < TimeSpan.FromMinutes(maxTimeMinutes))
                {
                    return true;
                }
            }
            return false;
        }
    }
}