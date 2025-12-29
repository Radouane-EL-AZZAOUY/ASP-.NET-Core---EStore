using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TP1.DataLayer;
using TP1.DataLayer.Interfaces;
using TP1.DataLayer.UnitOfWork;
using TP1.Services;
using TP1.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Database context
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cache configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    // Configure Redis with both IDistributedCache and direct connection for advanced features
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "E-Store:";
    });
    
    // Add direct Redis connection for pattern-based operations and advanced features
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse(redisConnectionString);
        configuration.AbortOnConnectFail = false; // Resilient connection
        configuration.ConnectRetry = 3;
        configuration.ConnectTimeout = 5000;
        configuration.SyncTimeout = 5000;
        
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Connecting to Redis at: {ConnectionString}", redisConnectionString);
        
        return ConnectionMultiplexer.Connect(configuration);
    });
}
else
{
    // Fallback to in-memory cache for development without Redis
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => null!); // No Redis connection
}

// Register Cache Service
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();

// Register RAG Service for product-based chatbot
builder.Services.AddScoped<IRAGService, RAGService>();

// Register Ollama Chat Service with RAG support
builder.Services.AddHttpClient<OllamaService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
builder.Services.AddScoped<IOllamaService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(OllamaService));
    var logger = sp.GetRequiredService<ILogger<OllamaService>>();
    var ragService = sp.GetRequiredService<IRAGService>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    
    return new OllamaService(httpClient, logger, ragService, configuration);
});

var app = builder.Build();  

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DBContext>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Seed products
        await DataSeeder.SeedProductsAsync(context, environment);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
