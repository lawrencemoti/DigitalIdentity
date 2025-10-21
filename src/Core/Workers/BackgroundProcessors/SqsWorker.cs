using Amazon.SQS;
using Amazon.SQS.Model;
using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Models;
using ApplicationCore.Models.SQS;
using Infrastructure.Data;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using static ApplicationCore.Models.SQS.SqsMessages;

namespace BackgroundProcessors;

public class SqsWorker : BackgroundService
{
    private readonly ILogger<SqsWorker> _logger;

    private readonly IAmazonSQS _sqs; 
    private readonly IServiceProvider _sp; 
    private readonly IHttpClientFactory _http;
    private readonly IIdentityVerificationAgent _idvAgent;

    private readonly string _queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL")!;

    public SqsWorker(ILogger<SqsWorker> logger, IAmazonSQS sqs, 
        IServiceProvider sp, IHttpClientFactory http, IIdentityVerificationAgent idvAgent)
    {
        _logger = logger;
        _sqs = sqs;
        _sp = sp;
        _http = http;
       _idvAgent = idvAgent;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            try
            {

                var sqsResp = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20,
                    VisibilityTimeout = 60
                }, stoppingToken);

                // No messages, continue polling
                foreach (var m in sqsResp.Messages)
                {
                    try
                    {
                        var msg = JsonSerializer.Deserialize<IdentityMessage>(m.Body)!;
                        //using var scope = _sp.CreateScope();
                        //var db = scope.ServiceProvider.GetRequiredService<IdentityDBContext>();
                        // Fetch identity record in DB
                        //var rec = await db.Identities.FindAsync(new object?[] { msg.IdentityId }, stoppingToken);
                        //if (rec == null) { await _sqs.DeleteMessageAsync(_queueUrl, m.ReceiptHandle, stoppingToken); continue; }
                        if (msg == null || msg.IdentityId == Guid.Empty)
                        {
                            await _sqs.DeleteMessageAsync(_queueUrl, m.ReceiptHandle, stoppingToken);
                            continue;
                        }

                        //TODO: Call third party API to validate identity
                        var resp = msg.IsVerificationWithDHA
                            ? await _idvAgent.VerifyIdentityWithProviderSource(msg.IdentityId, stoppingToken)
                            : await _idvAgent.VerifyIdentityWithDHA(msg.IdentityId, stoppingToken);

                        // Callback requesting client with results
                        if (!string.IsNullOrWhiteSpace(resp.CallbackUrl))
                        {
                            try
                            {
                                using var http = _http.CreateClient();
                                http.Timeout = TimeSpan.FromSeconds(5);
                                var cb = await http.PostAsJsonAsync(resp.CallbackUrl,
                                    new { resp.Id, resp.IdentityNumber, resp.FirstName, resp.Surname, 
                                            resp.Status, resp.DeceasedStatus }, stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Callback failed for {Id}", resp.IdentityNumber);
                            }
                        }
                       await _sqs.DeleteMessageAsync(_queueUrl, m.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Processing failed");
                        // Let visibility timeout expire; after 5 receives DLQ gets it per redrive policy.
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQS polling error"); 
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
