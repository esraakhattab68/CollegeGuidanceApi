using CollegeGuideApi.Data.SeedingData;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Middlewares;
using CollegeGuideApi.Models.DbContexts;
using CollegeGuideApi.Models.Entities;
using CollegeGuideApi.Profiles;
using CollegeGuideApi.Sevices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // ??? ???????? Swagger
using System.Text;

namespace CollegeGuideApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMemoryCache();

            builder.Services.AddHttpContextAccessor();

            // Add services to the container.

            builder.Services.AddControllers();

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CollegeGuide API", Version = "v1" });

                // Define the BearerAuth security scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new string[] {}
                    }
                });
            });

            // Database Configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpClient();

            // Add Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
            builder.Services.AddScoped<IMailService, MailService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IFinancialAidService, FinancialAidService>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddDataProtection();

            // Configure Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                //var jwtSettings = builder.Configuration.GetSection("Jwt");
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // <<<<<< ????? ?? ??? Key ?? 100%
                                                                                                                          // ClockSkew = TimeSpan.Zero // ???? ????? ?? ?? ????? ??? ????? ?? ??? ?????? ???????? (??? testing)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated for: " + context.Principal.Identity.Name);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine("OnChallenge: " + context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();
            // Add CORS policy for frontend development (optional, but usually needed)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error"); // ????? endpoint ??????? ??????? ??????
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAllOrigins"); // Apply CORS policy
            app.UseRouting();
            app.UseAuthentication();

            app.UseMiddleware<JwtDenylistMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            // Database Seeding


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var env = services.GetRequiredService<IWebHostEnvironment>();
                try
                {
                    await DataSeeder.SeedAsync(context, env);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during data seeding.");
                }
            }

            app.Run();
        }
    }
}