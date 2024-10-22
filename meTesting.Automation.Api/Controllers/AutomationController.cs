using MediatR;
using meTesting.Aether.SDK;
using meTesting.Automation.Api.Event;
using meTesting.Bus.SDK;
using meTesting.HRM.Services;
using meTesting.TransactionIdGenerator;
using Microsoft.AspNetCore.Mvc;

namespace meTesting.Automation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AutomationController : ControllerBase
{

    private readonly ILogger<AutomationController> _logger;
    private readonly IMediator mediator;
    private readonly ILetterService letterService;
    private readonly NotifSender notifSender;

    public AutomationController(ILogger<AutomationController> logger,
        IMediator mediator,
        ILetterService letterService,
        NotifSender notifSender)
    {
        _logger = logger;
        this.mediator = mediator;
        this.letterService = letterService;
        this.notifSender = notifSender;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetInbox(int userId)
    {
        return Ok(letterService.GetInbox(userId));
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> GetLetter(int id)
    {
        return Ok(letterService.Get(id));
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Approve(int id, int userId)
    {
        var l = letterService.Approve(id, userId);
        await mediator.Send(new LetterStateChangeEventArg
        {
            TransactionId = l.TransactionId,
            LetterId = id,
            NewState = LetterState.Approved,
        });
        return Ok(letterService.Get(id));
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Sign(int id, int userId)
    {
        var l = letterService.Sign(id, userId);
        await mediator.Send(new LetterStateChangeEventArg
        {
            TransactionId = l.TransactionId,
            LetterId = id,
            NewState = LetterState.Signed
        });
        return Ok();
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Reject(int id, int userId)
    {
        var l = letterService.Reject(id, userId);
        await mediator.Send(new LetterStateChangeEventArg
        {
            TransactionId = l.TransactionId,
            LetterId = id,
            NewState = LetterState.Rejected
        });
        return Ok();
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Archive(int id, int userId)
    {
        letterService.Archive(id, userId);
        return Ok(letterService.Get(id));
    }
}

public class CreateLetterRequest : IRequest<CreateLetterResult>
{
    public int UserId { get; set; }
    public int PositionId { get; set; }
    public string Body { get; set; }
    public string FlowId { get; set; }
    public string? TransactionId { get; set; }
}
public class CreateLetterResult
{
}

public class CreateLetterHandler(IChartService chartService,
ILetterService letterService,
ILogger<CreateLetterHandler> logger,
TrGen trGen,
Publisher publisher
) : IRequestHandler<CreateLetterRequest, CreateLetterResult>
{
    public async Task<CreateLetterResult?> Handle(CreateLetterRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(CreateLetterHandler)} has been fired");

        var f = FlowStore.Find(request.FlowId);

        if (f is null)
            throw new ArgumentNullException($"{nameof(f)} need a valid and defined value");

        letterService.CreateLetter(new Letter()
        {
            UserId = f.Approve,
            Body = request.Body,
            ApprovesBy = f.Approve,
            SignsBy = f.Sign,
            TransactionId = request.TransactionId,
        });

        return default;
    }
}
public class Flow
{
    public string Id { get; set; }
    public int Approve { get; set; }
    public int Sign { get; set; }
}

public static class FlowStore
{
    static Dictionary<string, Flow> _store = new()
    {
        {
            "SomeFlow#1",
            new Flow()
            {
                Approve = 1,
                Sign = 1
            }
        }
    };
    public static Flow? Find(string key) => _store.GetValueOrDefault(key);
}


