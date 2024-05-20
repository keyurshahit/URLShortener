using Serilog;
using URLShortener;
using URLShortener.Common.Settings;
using URLShortener.Common.Util;
using URLShortener.Services;

var builder = WebApplication.CreateBuilder(args);


// Configure logger (SeriLog)
//
Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithThreadId()
            .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();


// Add services to the container
//
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<Settings>(options => builder.Configuration.GetSection("Settings").Bind(options));
builder.Services.AddSwaggerGen();

// add hosted service
//
builder.Services.AddHostedService<Worker>();

// add singleton services
//
builder.Services.AddSingleton<IUrlShortenerService, UrlShortenerService>();
builder.Services.AddSingleton<IEncoder, Base62Encoder>();


// add cors policy
//
builder.Services.AddCors(options =>
{
    // in a production env this will have to be configured differently
    //
    options.AddPolicy("AllowAllOrigins",
          builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// build the app
//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();
