using BitByBit.Business.Extensions;
using BitByBit.Core.Models;
using BitByBit.DataAccess.Context;
using BitByBit.Entities.Models;
using BitByBit.Entities.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BitByBity.API
{
    public class Program
    {
        public static async Task Main(string[] args)  // ✅ async Main
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

            // Identity Services - ✅ DÜZƏLDİLDİ
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;      // ✅ Relaxed for testing
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;  // ✅ Development üçün false
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;  // ✅ Development üçün artırıldı
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
                options.AddPolicy("ModeratorOnly", policy => policy.RequireRole("Moderator"));
                options.AddPolicy("EmailConfirmed", policy => policy.RequireClaim("emailConfirmed", "True"));
            });

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "http://localhost:5174",
                            "http://localhost:8080",
                            "http://localhost:4200",
                            "https://localhost:3000",
                            "https://localhost:5173",
                            "https://localhost:5174"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });

                options.AddPolicy("DevelopmentOnly", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin)) return false;
                        try
                        {
                            var uri = new Uri(origin);
                            return uri.Host == "localhost" || uri.Host == "127.0.0.1";
                        }
                        catch
                        {
                            return false;
                        }
                    })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            // ✅ ROLE VƏ ADMIN USER YARATMA
            await SeedRolesAndAdminUserAsync(app.Services);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BitByBit API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors("DevelopmentOnly");
            }
            else
            {
                app.UseCors("AllowFrontend");
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Test endpoints
            app.MapGet("/", () => new {
                Message = "BitByBit API işləyir! ",
                Time = DateTime.Now,
                JWT = "Enabled ✅",
                Auth = "Bearer Token Required ",
                CORS = "Fixed for Frontend ",
                AdminUser = "admin@test.com / Admin123!" //  Admin user info
            });

            app.MapGet("/test", () => new {
                Message = "API test",
                Time = DateTime.Now,
                Environment = app.Environment.EnvironmentName
            });

            app.MapGet("/jwt-test", () => new {
                Message = "JWT Test Endpoint",
                JwtSettings = new
                {
                    Issuer = jwtSettings?.Issuer,
                    Audience = jwtSettings?.Audience,
                    ExpiryHours = jwtSettings?.ExpiryHours
                }
            });

            app.MapGet("/cors-test", () => new {
                Message = "CORS Test Successful! ",
                Time = DateTime.Now,
                Headers = "Allow-Credentials: true"
            });

            app.Run();
        }

        // ✅ ROLE SEEDING VƏ ADMIN USER YARATMA METODu
        private static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // ✅ Database ensure created
                await context.Database.EnsureCreatedAsync();

                // ✅ Identity Role-ları yarat
                string[] roleNames = { "User", "Admin", "Moderator" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new IdentityRole(roleName);
                        await roleManager.CreateAsync(role);
                        Console.WriteLine($"✅ Role created: {roleName}");
                    }
                }

                // ✅ Test admin istifadəçisi yarat
                var adminEmail = "admin@test.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "System",
                        LastName = "Admin",
                        Role = UserRole.Admin,  // ✅ Custom enum role
                        PhoneNumberConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin"); // ✅ Identity role
                        Console.WriteLine($" Admin user created: {adminEmail} / Admin123!");
                    }
                    else
                    {
                        Console.WriteLine($" Admin user creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    // ✅ Mövcud admin user-ə role təyin et (əgər yoxdursa)
                    var userRoles = await userManager.GetRolesAsync(adminUser);
                    if (!userRoles.Contains("Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($" Admin role added to existing user: {adminEmail}");
                    }

                    // ✅ Custom enum role update et
                    if (adminUser.Role != UserRole.Admin)
                    {
                        adminUser.Role = UserRole.Admin;
                        await userManager.UpdateAsync(adminUser);
                        Console.WriteLine($" Custom role updated for user: {adminEmail}");
                    }
                }

                // ✅ Test regular user yarat
                var userEmail = "user@test.com";
                var regularUser = await userManager.FindByEmailAsync(userEmail);

                if (regularUser == null)
                {
                    regularUser = new User
                    {
                        UserName = userEmail,
                        Email = userEmail,
                        EmailConfirmed = true,
                        FirstName = "Test",
                        LastName = "User",
                        Role = UserRole.User,  // ✅ Custom enum role
                        PhoneNumberConfirmed = true
                    };

                    var result = await userManager.CreateAsync(regularUser, "User123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(regularUser, "User"); // ✅ Identity role
                        Console.WriteLine($" Regular user created: {userEmail} / User123!");
                    }
                }

                Console.WriteLine(" Role seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error during role seeding: {ex.Message}");
            }
        }
    }
}