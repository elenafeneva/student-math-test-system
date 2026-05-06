using MathTaskValidator.Api;
using MathTaskValidator.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MathTaskValidator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();

//Lowercase Urls.
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
    options.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    options.DescribeAllParametersInCamelCase();
    options.UseInlineDefinitionsForEnums();
  });

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings); // Map Configuration values to appSettings object instance.
builder.Services.AddSingleton(typeof(AppSettings), appSettings); // Register to the container.

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(appSettings.ConnectionString));

builder.Services.Configure<AppSettings>(builder.Configuration);

//DI
builder.Services.AddScoped<IUploadDataService, UploadDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
