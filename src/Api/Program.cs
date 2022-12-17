using Carter;
using FluentValidation;
using LyricsApp.Api.Extensions;
using LyricsApp.Application;
using LyricsApp.Application.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilog();

builder.Services.AddCustomCors();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddCarter();
builder.Services.AddAutoMapper(typeof(Application));
builder.Services.AddMediator();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Application));

var app = builder.Build();

app.UseCors(AppConstants.CorsPolicy);
app.UseStaticFiles();
app.MapSwagger();
app.MapCarter();
app.Run();