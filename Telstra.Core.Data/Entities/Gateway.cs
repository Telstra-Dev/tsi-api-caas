namespace Telstra.Core.Data.Entities
{
    public class Gateway : DeviceBase
    {
            public Gateway()
            {
                Type = DeviceType.gateway.ToString();
            }
            public DeviceType DeviceType { get; set; }
            public GatewayMetadata Metadata { get; set; } 
    }
}