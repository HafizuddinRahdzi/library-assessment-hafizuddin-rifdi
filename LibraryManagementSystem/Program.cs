
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace LibraryManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog(); // Replace default logging with Serilog

            // Add services to the container.
            // Register EF Core with SQL Server
            builder.Services.AddDbContext<DBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //options.Authority = builder.Configuration["Authentication:Authority"];
                    //options.Audience = builder.Configuration["Authentication:Audience"];
                    //options.TokenValidationParameters.ValidateIssuer = true;
                    //options.TokenValidationParameters.ValidateAudience = true;
                    //options.TokenValidationParameters.ValidateLifetime = true;

                    //options.RequireHttpsMetadata = false;
                    options.Authority = builder.Configuration["Authentication:Authority"];
                    options.Audience = builder.Configuration["Authentication:Audience"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,

                        // ?? Map Keycloak's "sub" claim to NameIdentifier
                        NameClaimType = "sub",
                        RoleClaimType = "roles"
                    };

                    options.RequireHttpsMetadata = false;
                });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<MemberProvisioningMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
