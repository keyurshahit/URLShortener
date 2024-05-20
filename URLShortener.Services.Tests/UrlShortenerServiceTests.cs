using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using URLShortener.Common.Settings;
using URLShortener.Common.Util;

namespace URLShortener.Services.Tests
{
    /// <summary>
    /// Unit tests for all public methods of URLShortenerService.cs
    /// </summary>
    [TestClass()]
    public class UrlShortenerServiceTests
    {
        private IUrlShortenerService _urlShortenerService;
        private IEncoder _encoder;

        [TestInitialize()]
        public void UrlShortenerServiceTestInitialize()
        {
            var mockLogger = new Mock<ILogger<UrlShortenerService>>(MockBehavior.Loose);

            var settings = new Settings() { Base62Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", EncodingLength = 7 };
            var mockOptions = new Mock<IOptions<Settings>>(MockBehavior.Strict);
            mockOptions.Setup(x => x.Value).Returns(settings);

            _encoder = new Base62Encoder(mockOptions.Object);
            _urlShortenerService = new UrlShortenerService(mockLogger.Object, _encoder);
        }

        [TestMethod()]
        public void CreateShortUrlTest()
        {
            try
            {
                var shortUrl = _urlShortenerService.CreateShortUrl("www.msdn1.com").Result;
                Assert.IsNotNull(shortUrl);
                Assert.AreEqual(7, shortUrl.Length);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod()]
        public void GetShortUrlTest()
        {
            try
            {
                var shortUrl = _urlShortenerService.GetShortUrl("www.msdn2.com").Result;
                Assert.AreEqual(shortUrl, "");
                Assert.AreEqual(0, shortUrl.Length);

                var url = _urlShortenerService.CreateShortUrl("www.msdn2.com").Result;
                shortUrl = _urlShortenerService.GetShortUrl("www.msdn2.com").Result;
                Assert.IsNotNull(shortUrl);
                Assert.AreEqual(7, shortUrl.Length);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod()]
        public void GetOrCreateShortUrlTest1()
        {
            try
            {
                var shortUrl = _urlShortenerService.GetOrCreateShortUrl("www.msdn3.com").Result;
                Assert.IsNotNull(shortUrl);
                Assert.AreEqual(7, shortUrl.Length);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod()]
        public void GetOrCreateShortUrlTest2()
        {
            try
            {
                string myshorturl = "shorturl1";
                var shortUrl = _urlShortenerService.GetOrCreateShortUrl("www.msdn4.com", myshorturl).Result;
                Assert.IsNotNull(shortUrl);
                Assert.AreEqual(shortUrl, myshorturl);
                Assert.AreEqual(myshorturl.Length, shortUrl.Length);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod()]
        public void GetLongUrlTest()
        {
            string mylongurl = "www.msdn5.com";
            var shortUrl = _urlShortenerService.CreateShortUrl(mylongurl).Result;
            Assert.IsNotNull(shortUrl);

            var longurl = _urlShortenerService.GetLongUrl(shortUrl).Result;
            Assert.IsNotNull(longurl);
            Assert.AreEqual(mylongurl, longurl);
        }

        [TestMethod()]
        public void GetUrlStatsTest()
        {
            string mylongurl = "www.msdn6.com";
            var shortUrl = _urlShortenerService.CreateShortUrl(mylongurl).Result;
            var longurl = _urlShortenerService.GetLongUrl(shortUrl).Result;
            var stats = _urlShortenerService.GetUrlStats(shortUrl).Result;
            Assert.IsNotNull(stats);
            Assert.AreEqual(1, stats.HitRate);
        }

        [TestMethod()]
        public void DeleteShortUrlTest()
        {
            string mylongurl = "www.msdn7.com";
            var shortUrl = _urlShortenerService.CreateShortUrl(mylongurl).Result;
            var removed = _urlShortenerService.DeleteShortUrl(shortUrl).Result;
            Assert.IsTrue(removed);
        }
    }
}