using MediatR;
using meTesting.Automation.Api.Controllers;
using meTesting.Bus.SDK;
using meTesting.HRM.Services;
using meTesting.Sauron;
using Serilog.Context;
using System.Text.Json;

public class AutoHandler(IServiceProvider services, ILogger<AutoHandler> logger) : IOnRecieveEvent
{
    static Func<Message, IBaseRequest?> Resolver = a =>
    {
        return a.Type switch
        {
            "ReassignmentRequest" => ResolveForReassignmentRequest(a),

            _ => null
        };

        static CreateLetterRequest ResolveForReassignmentRequest(Message msg)
        {
            var data = JsonSerializer.Deserialize<ReassignmentRequest>(msg.Body, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });



            return new CreateLetterRequest()
            {
                UserId = data.CreatorId,
                PositionId = data.PositionId,
                Body = $"Please accept reassignment of User {data.UserId} for position {data.PositionId}",
                FlowId = "SomeFlow#1",
                TransactionId = data.TrId,
            };
        }
    };

    public async Task Do(Message message)
    {
        Console.WriteLine(JsonSerializer.Serialize(message));

        if (Resolver(message) is { } a)
        {
            logger.LogInformation("automation get a new event");
            using var scop = services.CreateScope();
            IMediator mediator = scop.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(a);
            logger.LogInformation("automation served the new event");

        }
    }
}

