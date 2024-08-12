// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using SqliteFulltextSearch.Api.Constants;
using SqliteFulltextSearch.Api.Infrastructure.Errors;
using SqliteFulltextSearch.Api.Infrastructure.Errors.Translators;
using SqliteFulltextSearch.Api.Infrastructure.Authentication;
using SqliteFulltextSearch.Api.Configuration;
using SqliteFulltextSearch.Api.Services;
using SqliteFulltextSearch.Database;
using SqliteFulltextSearch.Api.Endpoints;
using Microsoft.Data.Sqlite;
using SqliteFulltextSearch.Api.Hosting;
using SqliteFulltextSearch.Database.Infrastructure;
using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Readers;
using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors;
using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing;

public partial class Program {
    private static async Task Main(string[] args)
    {
        // We will log to %LocalAppData%/GitClub to store the Logs, so it doesn't need to be configured 
        // to a different path, when you run it on your machine.
        string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitClub");

        // We are writing with RollingFileAppender using a daily rotation, and we want to have the filename as 
        // as "GitClub-{Date}.log", the date will be set by Serilog automagically.
        string logFilePath = Path.Combine(logDirectory, "GitClub-.log");

        // Configure the Serilog Logger. This Serilog Logger will be passed 
        // to the Microsoft.Extensions.Logging LoggingBuilder using the 
        // LoggingBuilder#AddSerilog(...) extension.
        Log.Logger = new LoggerConfiguration()
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware"))
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Logging
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            // Database
            builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder()
                {
                    ConnectionString = builder.Configuration.GetConnectionString("ApplicationDatabase"),
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Cache = SqliteCacheMode.Private
                };

                var connectionString = connectionStringBuilder.ToString();

                options
                    .EnableSensitiveDataLogging()
                    .UseSqlite(connectionString);
            });

            // Add Hosted Services
            builder.Services.AddHostedService<DatabaseMigrationBackgroundService>();

            // Authentication
            builder.Services.AddScoped<CurrentUser>();
            builder.Services.AddScoped<IClaimsTransformation, CurrentUserClaimsTransformation>();

            // CORS
            builder.Services.AddCors(options =>
            {
                var allowedOrigins = builder.Configuration
                    .GetSection("AllowedOrigins")
                    .Get<string[]>();

                if (allowedOrigins == null)
                {
                    throw new InvalidOperationException("AllowedOrigins is missing in the appsettings.json");
                }

                options.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            // Add Exception Handling
            builder.Services.AddSingleton<IExceptionTranslator, DefaultExceptionTranslator>();
            builder.Services.AddSingleton<IExceptionTranslator, ApplicationErrorExceptionTranslator>();
            builder.Services.AddSingleton<IExceptionTranslator, InvalidModelStateExceptionTranslator>();

            builder.Services.Configure<ExceptionToErrorMapperOptions>(o =>
            {
                o.IncludeExceptionDetails = builder.Environment.IsDevelopment() || builder.Environment.IsStaging();
            });

            builder.Services.AddSingleton<ExceptionToErrorMapper>();

            builder.Services.AddExceptionHandler<ApplicationErrorExceptionHandler>();

            // Infrastructure (Document Readers)
            builder.Services.AddSingleton<PdfDocumentReader>();
            builder.Services.AddSingleton<WordDocumentReader>();

            // Infrastructure (Document Processors)
            builder.Services.AddSingleton<IDocumentProcessor, PdfDocumentProcessor>();
            builder.Services.AddSingleton<IDocumentProcessor, WordDocumentProcessor>();
            builder.Services.AddSingleton<IDocumentProcessor, TextDocumentProcessor>();

            // Infrastructure (Document Processing Engine)
            builder.Services.AddSingleton<DocumentProcessingEngine>();

            // Application Services
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<DocumentService>();
            builder.Services.AddSingleton<SqliteSearchService>();
            builder.Services.AddSingleton<DatabaseManagement>();

            builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // Cookie Authentication
            builder.Services
                // Using Cookie Authentication between Frontend and Backend
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                // We are going to use Cookies for ...
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax; // We don't want to deal with CSRF Tokens

                    options.Events.OnRedirectToLogin = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAntiforgery();

            // Add Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.RequireUserRole, policy => policy.RequireRole(Roles.User));
                options.AddPolicy(Policies.RequireAdminRole, policy => policy.RequireRole(Roles.Administrator));
            });

            // Add the Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.OnRejected = (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    return ValueTask.CompletedTask;
                };

                options.AddPolicy(Policies.PerUserRatelimit, context =>
                {
                    var username = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    return RateLimitPartition.GetTokenBucketLimiter(username, key =>
                    {
                        return new()
                        {
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            AutoReplenishment = true,
                            TokenLimit = 100,
                            TokensPerPeriod = 100,
                            QueueLimit = 100,
                        };
                    });
                });
            });

            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // CORS
            app.UseCors("CorsPolicy");

            app.UseRateLimiter();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseRateLimiter();

            // Add API Endpoints
            app.MapSearchEndpoints();

            app.Run();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "An unhandeled exception occured.");
        }
        finally
        {
            // Wait 0.5 seconds before closing and flushing, to gather the last few logs.
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            await Log.CloseAndFlushAsync();
        }
    }
}

public partial class Program { }