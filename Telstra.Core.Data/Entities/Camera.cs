namespace Telstra.Core.Data.Entities
{
    public class Camera : DeviceBase
    {
            public Camera()
            {
                Type = DeviceType.camera.ToString();
            }
            public DeviceType DeviceType { get; set; }
            public CameraMetadata Metadata { get; set; } 

    }
}
