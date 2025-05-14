using Microsoft.OpenApi.Models;
using WorkforceAPI.Services;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

 //CONFIGURATION SETUP 
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

//  DATABASE CONFIGURATION 
builder.Services.AddDbContext<EmployeeContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// JSON CONFIGURATION
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

//  CORS CONFIGURATION 
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://yourproductiondomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// KESTREL CONFIGURATION 
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);

    // HTTP Configuration
    serverOptions.Listen(IPAddress.Any, 5228, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });

    // HTTPS Configuration
    serverOptions.Listen(IPAddress.Any, 7228, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

// SWAGGER CONFIGURATION
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WorkforceAPI",
        Version = "v1.0",
        Description = "Workforce Management API",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@workforce.com",
            Url = new Uri("https://support.workforce.com")
        },
        License = new OpenApiLicense { Name = "MIT License" }
    });

    try
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not load XML comments: {ex.Message}");
    }
});
builder.Services.AddScoped<EmployeeService>();

var app = builder.Build();

// EXCEPTION HANDLING
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        await context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred",
            Detailed = app.Environment.IsDevelopment() ? exception?.Message : null,
            Path = exceptionHandlerPathFeature?.Path,
            StackTrace = app.Environment.IsDevelopment() ? exception?.StackTrace : null
        });

        Console.WriteLine($"Unhandled exception: {exception}");
    }
});

//  DEVELOPMENT CONFIG
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkforceAPI v1");
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.DefaultModelsExpandDepth(-1);
        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new { theme = "monokai" };
    });

    app.UseDeveloperExceptionPage();
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseHsts();
    app.UseCors("ProductionPolicy");
}

// SECURITY HEADERS 
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Feature-Policy", "accelerometer 'none'; camera 'none'");

    await next();
});

//  MIDDLEWARE PIPELINE 
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
    ForwardLimit = 10,
    KnownProxies = { IPAddress.Parse("10.0.0.100") }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//  REQUEST LOGGING 
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    var requestId = Guid.NewGuid().ToString();
    context.Items["RequestId"] = requestId;

    Console.WriteLine($"[{startTime:HH:mm:ss.fff}] [{requestId}] {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");

    try
    {
        await next();
    }
    finally
    {
        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] [{requestId}] Completed {context.Response.StatusCode} in {duration.TotalMilliseconds}ms");
    }
});

//  HEALTH CHECK 
app.MapGet("/health", async (EmployeeContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        return Results.Ok(new
        {
            status = canConnect ? "Healthy" : "Degraded",
            database = canConnect ? "Connected" : "Disconnected",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = app.Environment.EnvironmentName
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Service Unhealthy",
            detail: $"Database connection failed: {ex.Message}",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

//  APPLICATION START 
app.MapControllers();

try
{
    Console.WriteLine("=== WorkforceAPI Starting ===");
    Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
    Console.WriteLine($"Database: {builder.Configuration.GetConnectionString("DefaultConnection")}");
    Console.WriteLine($"Listening on: http://*:5228 and https://*:7228");

    // Test database connection
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
        var canConnect = await db.Database.CanConnectAsync();
        Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");

        if (canConnect)
        {
            Console.WriteLine("Applying pending migrations...");
            await db.Database.MigrateAsync();
        }
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex}");
}
finally
{
    Console.WriteLine("=== WorkforceAPI Stopped ===");
}
