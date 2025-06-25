using System.Text;
using System.Text.Json.Serialization;
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
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
                .AddJsonOptions(options=>
                { 
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

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

        //repository
        builder.Services.AddScoped<IListingCasesRepository, ListingCasesRepository>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        //service
        builder.Services.AddScoped<IListingCasesService, ListingCasesService>();
        builder.Services.AddScoped<IAgentListingCaseValidator, AgentListingCaseValidator>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddSingleton<GlobalExceptionHandler>();

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



