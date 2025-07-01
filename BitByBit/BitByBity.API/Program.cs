using BitByBit.Business.Extensions;
using BitByBit.Core.Models;
using BitByBit.DataAccess.Context;
using BitByBit.Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BitByBity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "BitByBit API",
                    Version = "v1",
                    Description = "BitByBit Platform API with JWT Authentication"
                });

                // JWT Authorization için Swagger konfiqurasiyası
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Database Connection
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 🔐 JWT Settings Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            // JWT Authentication Configuration
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Development üçün
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings?.ValidateIssuer ?? true,
                    ValidateAudience = jwtSettings?.ValidateAudience ?? true,
                    ValidateLifetime = jwtSettings?.ValidateLifetime ?? true,
                    ValidateIssuerSigningKey = jwtSettings?.ValidateIssuerSigningKey ?? true,
                    RequireExpirationTime = jwtSettings?.RequireExpirationTime ?? true,

                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "")),

                    ClockSkew = TimeSpan.FromMinutes(jwtSettings?.ClockSkewMinutes ?? 5)
                };

                // JWT Events
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"Authentication challenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });

            // Identity Services
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true; // Email confirmation required!
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Business Services (JWT service də burada əlavə olunur)
            builder.Services.AddBusinessServices(builder.Configuration);

            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
                options.AddPolicy("EmailConfirmed", policy => policy.RequireClaim("emailConfirmed", "True"));
            });

            // CORS (if needed for frontend)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Build app
            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BitByBit API V1");
                    c.RoutePrefix = string.Empty; // Swagger-i root-da göstər
                });
            }

            // Pipeline ORDER ÇOX MÜHİMDİR!
            app.UseHttpsRedirection();
            app.UseCors("AllowAll");        // CORS
            app.UseAuthentication();        // 🔐 Authentication (JWT) - AUTHORIZATION-dan ƏVVƏL
            app.UseAuthorization();         // 🛡️ Authorization - AUTHENTICATION-dan SONRA
            app.MapControllers();

            // Test endpoints
            app.MapGet("/", () => new {
                Message = "BitByBit API işləyir! 🚀",
                Time = DateTime.Now,
                JWT = "Enabled ✅",
                Auth = "Bearer Token Required 🔐"
            });

            app.MapGet("/test", () => new {
                Message = "API test",
                Time = DateTime.Now,
                Environment = app.Environment.EnvironmentName
            });

            // JWT Test endpoint (public)
            app.MapGet("/jwt-test", () => new {
                Message = "JWT Test Endpoint",
                JwtSettings = new
                {
                    Issuer = jwtSettings?.Issuer,
                    Audience = jwtSettings?.Audience,
                    ExpiryHours = jwtSettings?.ExpiryHours
                }
            });

            app.Run();
        }
    }
}