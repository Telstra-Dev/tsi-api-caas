namespace WCA.Consumer.Api.Models.StorageReponse
{
    public class BaseResponse
    {
        public string Exception { get; set; }
        public int Status { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsCompleted { get; set; }
        public int CreationOptions { get; set; }
        public string AsyncState { get; set; }
        public bool IsFaulted { get; set; }
    }
}