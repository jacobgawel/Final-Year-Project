using Microsoft.EntityFrameworkCore;
using Parking_Grpc.Mapper;
using Parking_Grpc.Services;
using Parking_Infrastructure.Data;
using Parking_Infrastructure.Mapper;
using Parking_Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// adding scopes
builder.Services.AddScoped<IParkingRepository, ParkingRepository>();
builder.Services.AddAutoMapper(typeof(ParkingProfileGrpc));
builder.Services.AddAutoMapper(typeof(ParkingProfile));

// adding database connection to the project
builder.Services.AddDbContext<ParkingDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Parking-Api"));
});

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ParkingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();