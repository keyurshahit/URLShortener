using Microsoft.Extensions.Options;
using URLShortener.Common.Settings;

namespace URLShortener
{
    /// <summary>
    /// Background worker host process
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly Settings _settings;

        /// <summary>
        /// Constructor init
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        public Worker(ILogger<Worker> logger, IConfiguration configuration, IOptions<Settings> options)
        {
            _logger = logger;
            _configuration = configuration;            
            _settings = options.Value;
        }

        /// <summary>
        /// Main execute method called by the framework
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"Starting URLShortner service...");
                //
                // Call any async bootstrapping/initialization task here
                //
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running worker: {ex}");
            }

            await Task.CompletedTask;
        }
    }
}
