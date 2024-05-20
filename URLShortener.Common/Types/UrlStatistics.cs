namespace URLShortener.Common.Types
{
    /// <summary>
    /// Type to capture url statistics
    /// </summary>
    public class UrlStatistics
    {
        public string LongUrl { get; set; }
        public string ShortUrl { get; set; }
        public long HitRate { get; set; }
        // ...more stats here...
    }
}