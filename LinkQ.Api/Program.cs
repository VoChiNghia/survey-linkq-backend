using LinkQ.Core.Configuration;
using LinkQ.Api.Framework.Infrastructure.Extensions;
using LinkQ.Api.factories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(ConfigurationDefaults.AppSettingsFilePath, true, true);
if (!string.IsNullOrEmpty(builder.Environment?.EnvironmentName))
{
    var path = string.Format(ConfigurationDefaults.AppSettingsEnvironmentFilePath, builder.Environment.EnvironmentName);
    builder.Configuration.AddJsonFile(path, true, true);
}
builder.Configuration.AddEnvironmentVariables();
//load application settings
builder.Services.ConfigureApplicationSettings(builder, !builder.Environment!.IsProduction());
builder.Services.AddScoped<ISurveyModelFactory, SurveyModelFactory>();
builder.Host.UseDefaultServiceProvider(options =>
{
    //don't validate the scopes, since at the app start and the initial configuration we need 
    //to resolve some services (registered as "scoped") through the root container
    options.ValidateScopes = false;
    options.ValidateOnBuild = true;
});

//Add services to the application and configure service provider
builder.Services.ConfigureApplicationServices(builder);

var app = builder.Build();

//Configure the application HTTP request pipeline
app.ConfigureRequestPipeline();
app.StartEngine();

app.Run();
