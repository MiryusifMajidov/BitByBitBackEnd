using BitByBit.Business.Mappings;
using BitByBit.Business.Services.Implementations;
using BitByBit.Business.Services.Interfaces;
using BitByBit.Core.Models;
using BitByBit.DataAccess.Repository.Interfaces;
using BitByBit.DataAccess.Repository.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitByBit.Business.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // ✅ Email Settings Configuration
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // 🔐 JWT Settings Configuration
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // Business Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();

            // 🔐 JWT Service - YENİ ƏLAVƏ EDİLDİ
            services.AddScoped<IJwtService, JwtService>();

            // ✅ REPOSITORY SERVİSLƏRİ - Data Access Layer
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // ✅ ƏLAVƏ EDİLƏN SERVİSLƏR - Controller xətalarını aradan qaldırmaq üçün
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IServicesService, ServicesService>();

            // Future services (if needed)
            // services.AddScoped<IFileService, FileService>();
            // services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}