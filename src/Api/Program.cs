using Carter;
using FluentValidation;
using LyricsApp.Api.Extensions;
using LyricsApp.Application;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Helpers;
using LyricsApp.Application.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilog();

builder.Services.AddJwt(builder.Configuration);

builder.Services.AddCustomCors();
builder.Services.AddHttpContextAccessor(); 
builder.Services.AddScoped<IHttpContextService, HttpContextService>();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddCarter();
builder.Services.AddAutoMapper(typeof(Application));
builder.Services.AddMediator();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Application));
builder.Services.AddApplicationServices(builder.Configuration);



var app = builder.Build();
app.DbInitialize();
app.UseCors(AppConstants.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapSwagger();
app.MapCarter();
app.Run();