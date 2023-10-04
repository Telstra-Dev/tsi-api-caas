using System;
using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class SiteStatus
    {
        public List<EdgeDeviceStatus> EdgeDevices { get; set; }

        public HealthStatusModel GetHealthStatus()
        {
            var health = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Site online",
                Action = "Expand site to review",
            };

            // Check gateways

            // Priority 3: Check gateway children count status
            if (EdgeDevices == null || EdgeDevices.Count == 0)
            {
                health.Code = HealthStatusCode.AMBER;
                health.Reason = "No gateways";
                health.Action = "Config in site menu";
                return health;
            }

            foreach (var edgeDevice in EdgeDevices)
            {
                var deviceHealth = edgeDevice.GetHealthStatus();

                // Priority 1: Check gateway children online status
                if (deviceHealth.Reason == "Gateway offline" || deviceHealth.Reason == "Camera(s) offline")
                {
                    health.Code = HealthStatusCode.RED;
                    health.Reason = "Devices are offline";
                    health.Action = "Expand site to review";
                    return health;
                }
                // Priority 2: Check gateway children review status
                else if (deviceHealth.Reason == "Data offline" || deviceHealth.Reason == "Configure cameras" || deviceHealth.Reason == "No cameras")
                {
                    health.Code = HealthStatusCode.AMBER;
                    health.Reason = "Review devices";
                    health.Action = "Expand site to review";
                }
            }

            // Priority 4: Assume asset is online.
            return health;
        }
    }
}