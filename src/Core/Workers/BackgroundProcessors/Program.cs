using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using ApplicationCore.Contracts.Infrastructure;
using BackgroundProcessors;
using Infrastructure.Data;
using Infrastructure.IDVAPIs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using static Infrastructure.Data.DbConnStrings;

var builder = Host.CreateApplicationBuilder(args);

var secretArn = Environment.GetEnvironmentVariable("DB_SECRET_ARN")!;
var queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL")!;
var baseUrl = Environment.GetEnvironmentVariable("THIRD_PARTY_BASE_URL")!;

var sm = new AmazonSecretsManagerClient(RegionEndpoint.AFSouth1);
var sec = await sm.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretArn });
var creds = JsonSerializer.Deserialize<DbSecret>(sec.SecretString!);

builder.Services.AddDbContext<IdentityDBContext>(opt =>
opt.UseMySql($"server={creds!.host};port={creds.port};database={creds.dbname};user={creds.username};password={creds.password}",
ServerVersion.AutoDetect($"server={creds!.host};port={creds.port};database={creds.dbname}"))
);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IAmazonSQS>(new AmazonSQSClient(RegionEndpoint.AFSouth1));
builder.Services.AddScoped<IIdentityVerificationAgent, IdentityVerificationAgent>();
builder.Services.AddHostedService<SqsWorker>();

var host = builder.Build();

//using (var scope = host.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<IdentityDBContext>();
//    await db.Database.MigrateAsync();
//}

await host.RunAsync();
