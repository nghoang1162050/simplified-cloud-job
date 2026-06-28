using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using api.Configurations;
using api.Entities;
using api.Models;
using api.Repositories;
using api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// Add configuration for AWS settings
builder.Services.AddOptions<AwsSettings>()
    .Bind(builder.Configuration.GetSection(AwsSettings.SectionName))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.BucketName), "AWS BucketName is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Region), "AWS Region is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.TargetEc2InstanceId), "AWS Target EC2 Instance ID is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.ApiBaseUrl), "AWS API Base URL is required.")
    .ValidateOnStart();

// Add external services for dependency injection
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
builder.Services.AddScoped<IStorageService, S3StorageService>();
builder.Services.AddScoped<IComputeExecutionService, AwsSsmComputeService>();

// Add database context for Entity Framework Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IJobRepository, JobRepository>();

// Add services for dependency injection
builder.Services.AddScoped<IJobServices, JobServices>();

// Add validation for request models using FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BillingSummaryFilterRequestValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Cloud Job API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();

app.MapControllers();

app.Run();
