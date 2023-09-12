using Newtonsoft.Json;

namespace WCA.Consumer.Api.Models
{
    public class RtspFeedRequestModel
    {
        [JsonProperty("commandName")]
        public string CommandName { get; set; } = "action";

        [JsonProperty("parameters")]
        public RequestParameters RequestParameters { get; set; } = new RequestParameters();
    }

    public class RequestParameters
    {
        [JsonProperty("connectionTimeoutInSeconds")]
        public int ConnectionTimeoutInSeconds { get; set; } = 1;

        [JsonProperty("responseTimeoutInSeconds")]
        public int ResponseTimeoutSeconds { get; set; } = 5;

        [JsonProperty("payload")]
        public RequestPayload Payload { get; set; } = new RequestPayload();
    }

    public class RequestPayload
    {
        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; } = "getImage";

        [JsonProperty("imageRequest")]
        public ImageRequest ImageRequest { get; set; } = new ImageRequest();
    }

    public class ImageRequest
    {
        [JsonProperty("image_quality")]
        public int ImageQuality { get; set; } = 30;

        [JsonProperty("image_annotation")]
        public int ImageAnnotation = 1;
    }
}
