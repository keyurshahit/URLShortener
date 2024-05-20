using URLShortener.Common.Types;

namespace URLShortener.Services
{
    /// <summary>
    /// url shortener service contract
    /// </summary>
    public interface IUrlShortenerService
    {
        Task<string> CreateShortUrl(string longUrl);
        Task<string> GetShortUrl(string longUrl);
        Task<string> GetOrCreateShortUrl(string longUrl);
        Task<string> GetOrCreateShortUrl(string longUrl, string shortUrl);
        Task<string> GetLongUrl(string shortUrl);
        Task<UrlStatistics> GetUrlStats(string shortUrl);
        Task<bool> DeleteShortUrl(string shortUrl);
    }
}