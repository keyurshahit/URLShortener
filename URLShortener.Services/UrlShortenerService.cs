using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using URLShortener.Common.Types;
using URLShortener.Common.Util;

namespace URLShortener.Services
{
    /// <summary>
    /// Url shortener service processing logic
    /// </summary>
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly ILogger<UrlShortenerService> _logger;
        private readonly IEncoder _encoder;
        private IDictionary<string, IDictionary<string, byte>> _longToShortUrlMap; // in a prod env this will have to be replaced with a distributed key/value caching framework with a subset of data being in in-memory cache with an appropriate eviction and expiry policy
        private IDictionary<string, string> _shortToLongUrlMap; // in a prod env this will have to be replaced with a distributed key/value caching framework with a subset of data being in in-memory cache with an appropriate eviction and expiry policy
        private IDictionary<string, UrlStatistics> _urlStatistics;
        private long _counter = 0; // this is not going to work in a distributed environment. A more advanced soln will be required (e.g. unique number generator service etc.)

        /// <summary>
        /// constructor init
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        public UrlShortenerService(ILogger<UrlShortenerService> logger, IEncoder encoder)
        {
            _logger = logger;
            _encoder = encoder;
            _longToShortUrlMap = new ConcurrentDictionary<string, IDictionary<string, byte>>();
            _shortToLongUrlMap = new ConcurrentDictionary<string, string>();
            _urlStatistics = new ConcurrentDictionary<string, UrlStatistics>();
        }

        /// <summary>
        /// creates a new short url for a given long url 
        /// this does not check for existing short url for a given long url and hence would create new unique short url each time for the given long url if called multiple times
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<string> CreateShortUrl(string longUrl)
        {
            // input validation
            //
            if (string.IsNullOrWhiteSpace(longUrl))
                throw new ArgumentNullException(nameof(longUrl));

            // generate new short url 
            //
            string shortUrl = _encoder.Encode(Interlocked.Increment(ref _counter));

            // store the mappings
            //
            StoreUrlMappings(longUrl, shortUrl);

            // return new short url
            //
            return Task.FromResult(shortUrl);
        }

        /// <summary>
        /// gets a shortUrl for a given longUrl else returns empty string
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<string> GetShortUrl(string longUrl)
        {
            // input validation
            //
            if (string.IsNullOrWhiteSpace(longUrl))
                throw new ArgumentNullException(nameof(longUrl));

            // if exists returns the first short url else return an empty string
            //
            if (_longToShortUrlMap.TryGetValue(longUrl, out var shortUrls))
                return Task.FromResult(shortUrls.First().Key);
            else
                return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// gets a short url for a given long url if exists (does not create a new short url)
        /// create a new short url for a given long url if does not exists
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        public async Task<string> GetOrCreateShortUrl(string longUrl)
        {
            // check if a shortUrl exists for the given longUrl
            //
            string shortUrl = await GetShortUrl(longUrl);

            // if short url does not exist. Create a new one.
            //
            if (string.IsNullOrEmpty(shortUrl))
                shortUrl = await CreateShortUrl(longUrl);

            return shortUrl;
        }

        /// <summary>
        /// gets or creates a shortUrl with custom input shortUrl else creates a new one
        /// </summary>
        /// <param name="longUrl"></param>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        public async Task<string> GetOrCreateShortUrl(string longUrl, string shortUrl)
        {
            // input validation
            //
            if (string.IsNullOrWhiteSpace(longUrl))
                throw new ArgumentNullException(nameof(longUrl));
            if (string.IsNullOrWhiteSpace(shortUrl))
                throw new ArgumentNullException(nameof(shortUrl));

            // check if the short url exists and if it does create a new one
            //            
            if (_shortToLongUrlMap.ContainsKey(shortUrl))
                shortUrl = await CreateShortUrl(longUrl);
            else
                StoreUrlMappings(longUrl, shortUrl);

            // retrun short url
            //
            return shortUrl;
        }

        /// <summary>
        /// gets a longUrl for a given shortUrl
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<string> GetLongUrl(string shortUrl)
        {
            // input validation
            //
            if (string.IsNullOrWhiteSpace(shortUrl))
                throw new ArgumentNullException(nameof(shortUrl));

            if (_shortToLongUrlMap.TryGetValue(shortUrl, out var longUrl))
            {
                // update stats
                //
                UpdateStats(shortUrl, longUrl);

                return Task.FromResult(longUrl);
            }
            else
                return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// gets url stats for a given shortUrl
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<UrlStatistics> GetUrlStats(string shortUrl)
        {
            // input validation
            //
            if (string.IsNullOrWhiteSpace(shortUrl))
                throw new ArgumentNullException(nameof(shortUrl));

            if (_urlStatistics.TryGetValue(shortUrl, out var urlStats))
                return Task.FromResult(urlStats);
            else
                return Task.FromResult(default(UrlStatistics));
        }

        /// <summary>
        /// deletes a given shortUrl
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<bool> DeleteShortUrl(string shortUrl)
        {
            // input validation
            //
            if (string.IsNullOrWhiteSpace(shortUrl))
                throw new ArgumentNullException(nameof(shortUrl));

            // check if short url exists and if yes remove it from mappings
            //
            if (_shortToLongUrlMap.TryGetValue(shortUrl, out var longUrl))
            {
                bool removedShort = _shortToLongUrlMap.Remove(shortUrl);
                bool removedLong = false;
                if (_longToShortUrlMap.TryGetValue(longUrl, out var shortUrls) && shortUrls.ContainsKey(shortUrl))
                    removedLong = shortUrls.Remove(shortUrl);

                return Task.FromResult(removedShort && removedLong);
            }

            // return
            //
            return Task.FromResult(false);
        }

        /// <summary>
        /// helper to store url mappings
        /// </summary>
        /// <param name="longUrl"></param>
        /// <param name="shortUrl"></param>
        private void StoreUrlMappings(string longUrl, string shortUrl)
        {
            _shortToLongUrlMap.Add(shortUrl, longUrl);
            if (!_longToShortUrlMap.ContainsKey(longUrl))
                _longToShortUrlMap.Add(longUrl, new ConcurrentDictionary<string, byte>());
            _longToShortUrlMap[longUrl].Add(shortUrl, 1);
        }

        /// <summary>
        /// helper to update stats for each url hit
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <param name="longUrl"></param>
        private void UpdateStats(string shortUrl, string longUrl)
        {
            if (!_urlStatistics.ContainsKey(shortUrl))
                _urlStatistics.Add(shortUrl, new UrlStatistics() { ShortUrl = shortUrl, LongUrl = longUrl });
            _urlStatistics[shortUrl].HitRate += 1;
        }
    }
}