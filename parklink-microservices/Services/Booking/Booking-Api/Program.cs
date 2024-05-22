using Amazon.S3;
using Booking_Api.Core;
using Booking_Infrastructure.GrpcServices;
using Booking_Infrastructure.Mapper;
using Booking_Infrastructure.Persistence.Data;
using Booking_Infrastructure.Persistence.Repositories;
using Booking_Infrastructure.S3;
using Booking_Infrastructure.Services;
using Booking_ServiceBus.Consumers;
using Booking_ServiceBus.Services;
using Hangfire;
using Hangfire.Pro.Redis;
using LocalStack.Client.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BookingDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Booking-Api"));
});

// Adding Hangfire Service
// Redis configuration for hangfire
var options = new RedisStorageOptions
{
    Prefix = "hangfire:"
};

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
    hangfireOptions.Queues = new[] { "booking" };
});

// adding MassTransmit configurations
builder.Services.AddMassTransit(ops =>
{
    // adding booking and parking queues/consumers
    ops.AddConsumer<ParkingDeletedConsumer>();
    ops.SetKebabCaseEndpointNameFormatter();
    ops.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"), "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// adding interfaces to the project for dependency injection
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IParkingGrpcServices, ParkingGrpcServices>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IS3UploadHelper, S3UploadHelper>();
builder.Services.AddScoped<IBookingSb, BookingSb>();

// AWS S3 configuration - Localstack
builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSServiceLocalStack<IAmazonS3>();

// AutoMapper Config
// Potential bug: Track mappings consistently to make sure that AutoMapper is mapping objects correctly
// this applies to all projects in the solution where automapper is involved e.g. API repositories
builder.Services.AddAutoMapper(typeof(BookingProfile));

// add grpc service and the service object
builder.Services.AddGrpcClient<ParkingProtoService.ParkingProtoServiceClient>
    (opt => opt.Address = new Uri(builder.Configuration.GetConnectionString("ParkingGrpc")!));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add authentication to the resource
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtBearerOptions =>
    {
        jwtBearerOptions.Authority = builder.Configuration["IdentityServiceUrl"];
        jwtBearerOptions.RequireHttpsMetadata = false;
        jwtBearerOptions.TokenValidationParameters.ValidateAudience = false;
        jwtBearerOptions.TokenValidationParameters.NameClaimType = "username";
    });

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});

// Configure the HTTP request pipeline.
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