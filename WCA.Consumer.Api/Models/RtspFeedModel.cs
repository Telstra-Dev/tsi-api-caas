using Newtonsoft.Json;
using System;

namespace WCA.Consumer.Api.Models
{
    public class RtspFeedModel
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public class Result
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }

    public class Payload
    {
        [JsonProperty("imageResponse")]
        public ImageResponse ImageResponse { get; set; }
    }

    public class ImageResponse
    {
        [JsonProperty("image")]
        public string Image { get; set; }
    }
}
