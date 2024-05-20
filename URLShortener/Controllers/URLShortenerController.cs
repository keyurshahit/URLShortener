using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Web;
using URLShortener.Common.Settings;
using URLShortener.Services;

namespace URLShortener.Controllers
{
    /// <summary>
    /// URL Shortener api interface
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class URLShortenerController : ControllerBase
    {
        private readonly ILogger<URLShortenerController> _logger;
        private readonly Settings _settings;
        private readonly IUrlShortenerService _urlShortenerService;

        /// <summary>
        /// constructor init
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public URLShortenerController(ILogger<URLShortenerController> logger, IOptions<Settings> settings, IUrlShortenerService urlProcessor)
        {
            _logger = logger;
            _settings = settings.Value;
            _urlShortenerService = urlProcessor;
        }

        /// <summary>
        /// post request to create a short url
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        [HttpPost("createShort")]
        public async Task<IActionResult> CreateShortUrl([FromBody] string longUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                var shortUrl = await _urlShortenerService.CreateShortUrl(longUrl);
                return Ok(shortUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(CreateShortUrl)} with params {nameof(longUrl)}: {longUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }

        /// <summary>
        /// post request to delete an existing short url
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        [HttpPost("deleteShort")]
        public async Task<IActionResult> DeleteShortUrl([FromBody] string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                if (await _urlShortenerService.DeleteShortUrl(shortUrl))
                    return Ok($"Deleted {nameof(shortUrl)}:{shortUrl}");
                else
                    return BadRequest($"Could not delete. Not found {nameof(shortUrl)}:{shortUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(DeleteShortUrl)} with params {nameof(shortUrl)}: {shortUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }

        /// <summary>
        /// post request to get or create (if not exists) a short url from a given long url
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        [HttpPost("getOrCreateShort")]
        public async Task<IActionResult> GetOrCreateShortUrl([FromBody] string longUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                var shortUrl = await _urlShortenerService.GetOrCreateShortUrl(longUrl);
                return Ok(shortUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(GetOrCreateShortUrl)} with params {nameof(longUrl)}: {longUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }

        /// <summary>
        /// post request to get or create (if not exists) a short url from a given long url and a custom short url
        /// </summary>
        /// <param name="longUrl"></param>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        [HttpPost("getOrCreateCustomShort")]
        public async Task<IActionResult> GetOrCreateShortUrl(string longUrl, [FromBody] string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl) || string.IsNullOrWhiteSpace(shortUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                var url = await _urlShortenerService.GetOrCreateShortUrl(longUrl, shortUrl);
                return Ok(url);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(GetOrCreateShortUrl)} with params - {nameof(longUrl)}: {longUrl}, {nameof(shortUrl)}: {shortUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }

        /// <summary>
        /// get request to get a long url from a given short url
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        [HttpGet("getLong/{shortUrl}")]
        public async Task<IActionResult> GetLongUrl(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                var longUrl = await _urlShortenerService.GetLongUrl(shortUrl);
                if (string.IsNullOrEmpty(longUrl))
                    return BadRequest($"No long url found for {nameof(shortUrl)}: {shortUrl}");
                else
                    return Ok(longUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(GetOrCreateShortUrl)} with params {nameof(shortUrl)}: {shortUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }

        /// <summary>
        /// get request to get a short url from a given long url
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        [HttpGet("getShort/{longUrl}")]
        public async Task<IActionResult> GetShortUrl(string longUrl)
        {
            longUrl = HttpUtility.UrlDecode(longUrl);
            if (string.IsNullOrWhiteSpace(longUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                var shortUrl = await _urlShortenerService.GetShortUrl(longUrl);
                if (string.IsNullOrEmpty(shortUrl))
                    return BadRequest($"No short url found for {nameof(longUrl)}: {longUrl}");
                else
                    return Ok(shortUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(GetShortUrl)} with params {nameof(longUrl)}: {longUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }

        /// <summary>
        /// get request to get statistics for a given short url
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("getStats/{shortUrl}")]
        public async Task<IActionResult> GetUrlStats(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return BadRequest("Url cannot be null or empty.");

            try
            {
                var stats = await _urlShortenerService.GetUrlStats(shortUrl);
                if (stats == null)
                    return BadRequest($"No statistics found for {nameof(shortUrl)}: {shortUrl}");
                else
                    return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling {nameof(GetOrCreateShortUrl)} with params {nameof(shortUrl)}: {shortUrl} => {ex}");
                return StatusCode(500, "An error occurred while processing request");
            }
        }
    }
}