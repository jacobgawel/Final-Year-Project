using Amazon.S3;
using Hangfire;
using Hangfire.Pro.Redis;
using LocalStack.Client.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Parking_Api.Core;
using Parking_Infrastructure.Data;
using Parking_Infrastructure.GrpcServices;
using Parking_Infrastructure.Mapper;
using Parking_Infrastructure.Repositories;
using Parking_Infrastructure.S3;
using Parking_Infrastructure.Services;
using Parking_ServiceBus.Consumer;
using Parking_ServiceBus.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ParkingDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("Parking-Api"));
});

builder.Services.AddScoped<IParkingRepository, ParkingRepository>();
builder.Services.AddScoped<IS3UploadHelper, S3UploadHelper>();
builder.Services.AddScoped<IParkingSb, ParkingSb>();
builder.Services.AddScoped<IBookingGrpcServices, BookingGrpcServices>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// AutoMapper configuration
builder.Services.AddAutoMapper(typeof(ParkingProfile));

// Hangfire configuration
var options = new RedisStorageOptions
{
    Prefix = "hangfire:"
};

GlobalConfiguration.Configuration.UseBatches();

builder.Services.AddHangfire(config =>
{
    config.UseRedisStorage(builder.Configuration.GetConnectionString("HangfireRedis"), options);
});

builder.Services.AddHangfireServer(hangfireOptions =>
{
    // More details in booking-api
    hangfireOptions.Queues = new[] { "parking" };
});


// MassTransmit configurations
builder.Services.AddMassTransit(ops =>
{
    ops.AddConsumer<BookingRecommendConsumer>();
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

// CORS POLICY - keep this as a reference just in case I need it later

// const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
//
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(name: myAllowSpecificOrigins,
//         build =>
//         {
//             build.WithOrigins().AllowAnyOrigin();
//         });
// });

// AWS S3 configuration - Localstack
builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSServiceLocalStack<IAmazonS3>();

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

// add grpc service and the service object
builder.Services.AddGrpcClient<BookingProtoService.BookingProtoServiceClient>
    (opt => opt.Address = new Uri(builder.Configuration.GetConnectionString("BookingGrpc")!));

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});

// CORS POLICY - invoke cors policy if its defined
// app.UseCors(myAllowSpecificOrigins);
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