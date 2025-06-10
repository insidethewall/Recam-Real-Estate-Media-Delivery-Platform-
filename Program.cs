using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ReacmSystemApi.Data;

namespace ReacmSystemApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ReacmDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ReacmDb")));

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

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.Run();

    }
}



