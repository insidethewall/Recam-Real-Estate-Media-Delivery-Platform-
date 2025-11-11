using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecamSystemApi.Data;
using RecamSystemApi.Exception;
using RecamSystemApi.Helper;
using RecamSystemApi.Models;
using RecamSystemApi.Services;



namespace RecamSystemApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddControllers()
                .AddJsonOptions(options=>
                { 
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

        builder.Services.Configure<MongoDbSettings>(
            options=>
            {
                options.ConnectionStrings = builder.Configuration["ConnectionStrings:MongoDb"] 
                    ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
                options.DatabaseName = builder.Configuration["DatabaseSettings:DatabaseName"] 
                    ?? throw new InvalidOperationException("MongoDB database name is not configured.");
            }
        );
        builder.Services.AddSingleton<MongoDbContext>();

        builder.Services.AddDbContext<ReacmDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ReacmDb")));
       
        builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ReacmDbContext>()
                .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = 
                    new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
                };
            });

        // solve circular reference problem
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        });

        //repository
        builder.Services.AddScoped<IListingCasesRepository, ListingCasesRepository>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        builder.Services.AddScoped<IGeneralRepository, GeneralRepository>();
        builder.Services.AddScoped<IListingCasesLogRepository, ListingCasesLogRepository>();
        builder.Services.AddScoped<IUserLogRepository,UserLogRepository>();
        //service
        builder.Services.AddScoped<IListingCasesService, ListingCasesService>();
        builder.Services.AddScoped<IAgentListingCaseValidator, AgentListingCaseValidator>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IMediaAssetService, MediaAssetService>();
        builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddSingleton<GlobalExceptionHandler>();
        builder.Services.AddSingleton(x =>
        {
            var config = x.GetRequiredService<IConfiguration>();
            var connectionString = config["AzureBlobStorage:ConnectionString"];
            return new BlobServiceClient(connectionString);
        });

        builder.Services.AddTransient<IEmailSender, EmailSender>();

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Reacm API",
                    Description = "An ASP.NET Core Web API for Reacm System",
                    Contact = new OpenApiContact
                    {
                        Name = "Toby Ren",
                        Url = new Uri("mailto:rxy550569417@gmail.com")
                    },
                }
                );

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token below prefixed with **Bearer**. Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
                });
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            }
        );

        var app = builder.Build();
        app.UseExceptionHandler(errorApp =>
                errorApp.Run(async context =>
                {
                    var exceptionHandler = context.RequestServices.GetRequiredService<GlobalExceptionHandler>();
                    await exceptionHandler.HandleExceptionAsync(context);
                }
            ));

        using (var scope = app.Services.CreateScope())
        { 
            var services = scope.ServiceProvider;
            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await RoleSeeder.SeedRolesAsync(services);
                await AdminSeeder.SeedAdminAsync(services);
            }
            catch (System.Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

  

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();

    }
}



