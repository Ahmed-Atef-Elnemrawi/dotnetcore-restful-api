using EventHub.EventManagement.API.Extensions;
using EventHub.EventManagement.API.Utility;
using EventHub.EventManagement.Application.Contracts.Infrastructure;
using EventHub.EventManagement.Application.Contracts.links;
using EventHub.EventManagement.Application.Models.ConfigurationModels;
using EventHub.EventManagement.Application.Service;
using EventHub.EventManagement.Infrastructure;
using EventHub.EventManagement.Presentation.ActionFilter;
using EventHub.EventManagement.Presistence;
using Microsoft.AspNetCore.HttpOverrides;
using NLog;

var builder = WebApplication.CreateBuilder(args);

//configure LogManager
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntergration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureDataShaperServiceManager();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();
builder.Services.AddScoped<IEntitiesLinkGeneratorManager, EntitiesLinkGeneratorManager>();
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.ConfigureEmailService();

builder.Services.AddAutoMapper(typeof(EventHub.EventManagement.Application.Profiles.MappingProfile).Assembly);

//Configure controllers
builder.Services.AddControllers(config =>
{
   config.RespectBrowserAcceptHeader = true;
   config.ReturnHttpNotAcceptable = true;
})
   .AddXmlDataContractSerializerFormatters()
   .AddApplicationPart(typeof(EventHub.EventManagement.Presentation.AssemblyReference).Assembly);

builder.Services.AddCustomMediaTypes();
builder.Services.ConfiugerVersioningService();
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureSwagger();


var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerManager>();

app.ConfigureExceptionHandler(logger);

if (app.Environment.IsProduction())
   app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(opt =>
{
   opt.SwaggerEndpoint("/swagger/v1/swagger.json", "EventHub API v1");
});
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
   ForwardedHeaders = ForwardedHeaders.All
});
app.UseCors("CorsPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
