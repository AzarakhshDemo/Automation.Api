using MediatR;
using meTesting.Bus.SDK;
using meTesting.Chart;
using meTesting.Personnel;
using meTesting.LetterSrv;
using meTesting.TransactionIdGenerator;
using meTesting.Aether.SDK;
using meTesting.GeneralHelpers;
using meTesting.Sauron;
using meTesting.Sandbaad.Sdk;
using meTesting.Shared.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigFor<SauronConfig>(builder.Configuration, out var conf);

builder.Services.ConfigFor<SandbaadConfig>(builder.Configuration, out var sandConf);

builder.Services.ConfigFor<EnvironmentConfig>(builder.Configuration, out var envConf);

builder.Services.AddSauron(builder.Configuration, conf!);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSandbaadClient(sandConf);

builder.Services.AddLogging(a =>
a.AddConsole());

builder.Services.AddMediatR(a => a.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

builder.Services.AddPersonelService();
builder.Services.AddLetterService();
builder.Services.AddChartService();
//builder.Services.AddPersonelAttrributeService();

builder.Services.AddServiceBus(a =>
{
    a.BaseUrl = "http://localhost:5106";
    a.Key = "AUTO_API";
}, a => new AutoHandler(a, a.GetRequiredService<ILogger<AutoHandler>>()));

builder.Services.AddAetherNotifFromDiscovery();
//{
//    a.BaseUrl = "https://localhost:7156";
//});


builder.Services.AddSingleton<TrGen>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSauron();
app.UseAuthorization();

app.MapControllers();

app.Run();
