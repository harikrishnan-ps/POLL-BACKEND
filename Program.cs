using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using poll_api.Application.Interfaces;
using poll_api.Application.Services;
using poll_api.Infrastructure.Data;
using poll_api.Infrastructure.Repositories;
using poll_api.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPollService, PollService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey)) throw new ArgumentNullException("Jwt:Key is missing in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// CORS for Angular App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    var maxRetries = 10;
    var retryDelay = TimeSpan.FromSeconds(5);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to the database and apply migrations... (Attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database on attempt {Attempt}.", i + 1);
            if (i == maxRetries - 1)
            {
                logger.LogCritical("Could not connect to the database after {MaxRetries} attempts. Application startup will fail.", maxRetries);
                throw;
            }
            Thread.Sleep(retryDelay);
        }
    }
}

app.Run();
