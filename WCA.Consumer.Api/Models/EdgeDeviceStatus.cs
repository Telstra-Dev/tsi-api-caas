using System;
using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class EdgeDeviceStatus
    {
        public int EdgeDeviceId { get; set; }
        public string LastHealthReadingTimestamp { get; set; }
        public List<LeafDeviceStatus> LeafDevices { get; set; }
        public int DeviceRecentlyOnlineMaxMinutes { get; set; }

        public HealthStatusModel GetHealthStatus()
        {
            var edgeDeviceHealth = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review",
            };

            // Priority 1: Check online status
            if (!CheckEdgeDeviceRecentlyOnline(LastHealthReadingTimestamp, DeviceRecentlyOnlineMaxMinutes))
            {
                edgeDeviceHealth.Code = HealthStatusCode.RED;
                edgeDeviceHealth.Reason = "Gateway offline";
                edgeDeviceHealth.Action = "Contact support";
            }
            else
            {
                // Check leaf devices

                // Priority 5: Check camera children count status
                if (LeafDevices == null || LeafDevices.Count == 0)
                {
                    edgeDeviceHealth.Code = HealthStatusCode.AMBER;
                    edgeDeviceHealth.Reason = "No cameras";
                    edgeDeviceHealth.Action = "Config in gateway menu";
                }

                foreach (var leafDevice in LeafDevices)
                {
                    var deviceHealth = leafDevice.GetHealthStatus();

                    // Priority 2: Check camera children online status
                    if (deviceHealth.Reason == "Camera offline")
                    {
                        edgeDeviceHealth.Code = HealthStatusCode.RED;
                        edgeDeviceHealth.Reason = "Camera(s) offline";
                        edgeDeviceHealth.Action = "Expand gateway to review";
                        break;
                    }
                    // Priority 3: Check camera children configuration status
                    else if (deviceHealth.Reason == "Configure camera")
                    {
                        edgeDeviceHealth.Code = HealthStatusCode.AMBER;
                        edgeDeviceHealth.Reason = "Configure cameras";
                        edgeDeviceHealth.Action = "Expand gateway to review";
                    }
                    // Priority 4: Check camera children data online status
                    else if (deviceHealth.Reason == "Data offline")
                    {
                        // Check we're not overriding a higher priority status.
                        if (deviceHealth.Reason != "No cameras" && deviceHealth.Reason != "Configure cameras")
                        {
                            edgeDeviceHealth.Code = HealthStatusCode.AMBER;
                            edgeDeviceHealth.Reason = "Data offline";
                            edgeDeviceHealth.Action = "Expand gateway to review";
                        }
                    }
                }
            }

            // Priority 6: Assume asset is online.
            return edgeDeviceHealth;
        }

        public static bool CheckEdgeDeviceRecentlyOnline(string lastActiveTime, int maxTimeMinutes)
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
    }
}