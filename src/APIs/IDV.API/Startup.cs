using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Mappings;
using Infrastructure.Data;
using Infrastructure.IDVAPIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using AutoMapper;
using ApplicationCore.Contracts.Core;
using ApplicationCore.Services;

//AWS 
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace IDV.API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        // Register MySqlConnection for Aurora MySQL
        //services.AddDbContext<IdentityDBContext>(options =>
        //options.UseMySql(Configuration.GetConnectionString("AuroraMySql"),
        //        ServerVersion.AutoDetect(Configuration.GetConnectionString("AuroraMySql"))));

        services.AddHttpClient<IIdentityVerificationAgent, IdentityVerificationAgent>(c =>
        {
            c.BaseAddress = new Uri(Configuration["DatanamixAPI:BaseUrl"]);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddTransient<IIDVService, IDVService>();

        services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            //app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/swagger/v1/swagger.json", "IDV API v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/health", () => Results.Ok(new { status = "ok" }));
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}