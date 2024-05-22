using Amazon.S3;
using Fines_Api.Core;
using Fines_Infrastructure.GrpcServices;
using Fines_Infrastructure.Mapper;
using Fines_Infrastructure.Persistence.Data;
using Fines_Infrastructure.Persistence.Repositories;
using Fines_Infrastructure.S3;
using Fines_Infrastructure.Services;
using Hangfire;
using Hangfire.Pro.Redis;
using LocalStack.Client.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add services to the container.
builder.Services.AddDbContext<FineDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Fines-Api"));
});

// Adding Hangfire Service
// Redis configuration for hangfire
var options = new RedisStorageOptions
{
    Prefix = "hangfire:"
};

// Use batches configuration for hangfire
GlobalConfiguration.Configuration.UseBatches();

// Use batches configuration for hangfire
GlobalConfiguration.Configuration.UseBatches();

builder.Services.AddHangfire(config =>
{
    // configuring redis as the storage location
    config.UseRedisStorage(builder.Configuration.GetConnectionString("HangfireRedis"), options);
});

builder.Services.AddHangfireServer(hangfireOptions =>
{
    // since we are using multiple hangfire workers, it is best to send specific jobs to certain queues
    // since some jobs might not be executed due to not having the correct dependencies.
    // This means that background jobs that are executed must be fired with queue annotations
    hangfireOptions.Queues = new[] { "fines" };
});

// adding interfaces to the project for dependency injection
builder.Services.AddAutoMapper(typeof(FinesProfile));

// Scopes for the interfaces
builder.Services.AddScoped<INotificationServices, NotificationServices>();
builder.Services.AddScoped<IFineRepository, FineRepository>();
builder.Services.AddScoped<IS3UploadHelper, S3UploadHelper>();

// AWS S3 configuration - Localstack
builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSServiceLocalStack<IAmazonS3>();

// add grpc service and the service object
builder.Services.AddGrpcClient<BookingProtoService.BookingProtoServiceClient>
    (opt => opt.Address = new Uri(builder.Configuration.GetConnectionString("BookingGrpc")!));

builder.Services.AddScoped<BookingGrpcServices>();

// Add authentication to the resource
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// migrate database if it doesnt exist
try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();