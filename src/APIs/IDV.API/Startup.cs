//AWS 
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ApplicationCore.Contracts.Core;
using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Mappings;
using ApplicationCore.Services;
using AutoMapper;
using Infrastructure.Data;
using Infrastructure.IDVAPIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System;
using System.Reflection;
using System.Text.Json;
using static Infrastructure.Data.DbConnStrings;

namespace IDV.API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // DB secret
    private readonly string secretArn = Environment.GetEnvironmentVariable("DB_SECRET_ARN")!;       
    // AWS Secrets Manager client
    private readonly IAmazonSecretsManager sm = new AmazonSecretsManagerClient(RegionEndpoint.AFSouth1);

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        var sec = sm.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretArn })
            .GetAwaiter().GetResult();
        
        var creds = sec.SecretString is not null
            ? JsonSerializer.Deserialize<DbSecret>(sec.SecretString)
            : null;

        // Register MySqlConnection for Aurora MySQL
        services.AddDbContext<IdentityDBContext>(opt =>
            opt.UseMySql($"server={creds!.host};port={creds.port};database={creds.dbname};user={creds.username};password={creds.password}",
            ServerVersion.AutoDetect($"server={creds!.host};port={creds.port}")));
        
        services.AddSingleton<IAmazonSimpleNotificationService>(
            new AmazonSimpleNotificationServiceClient(RegionEndpoint.AFSouth1));

        services.AddHttpClient<IIdentityVerificationAgent, IdentityVerificationAgent>(c =>
        {
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