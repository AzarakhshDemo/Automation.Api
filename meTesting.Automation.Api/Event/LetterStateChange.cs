using MediatR;
using meTesting.Aether.SDK;
using meTesting.Bus.SDK;

namespace meTesting.Automation.Api.Event;

public class LetterStateChangeEventHandler(Publisher publisher, 
    ILetterService letterService,
    ILogger<LetterStateChangeEventHandler> logger,
    NotifSender notifSender) : IRequestHandler<LetterStateChangeEventArg>
{
    public async Task Handle(LetterStateChangeEventArg request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{GetType().Name} Fired");
        
        var l = letterService.Get(request.LetterId);

        await notifSender.Send(l.UserId.ToString(), $"You have a letter waiting for {l.State}");

        if (string.IsNullOrEmpty(request.TransactionId))
            return;

        if (request.NewState is LetterState.Signed or LetterState.Rejected)
        {
            publisher.Publish(request);
        }
    }
}

public class LetterStateChangeEventArg : IRequest
{
    public int LetterId { get; set; }
    public string TransactionId { get; set; }
    public LetterState NewState { get; set; }
    public LetterState OldState { get; set; }
}



