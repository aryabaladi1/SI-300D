namespace SI_300D.Models
{
    public class NetworkStatistics
    {
        public long BytesReceived { get; set; }

        public long BytesSent { get; set; }

        public double DownloadBytesPerSecond { get; set; }

        public double UploadBytesPerSecond { get; set; }
    }
}
