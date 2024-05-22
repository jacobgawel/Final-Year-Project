using Booking_Grpc.Mapper;
using Booking_Grpc.Services;
using Booking_Infrastructure.Mapper;
using Booking_Infrastructure.Persistence.Data;
using Booking_Infrastructure.Persistence.Repositories;
using Booking_Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// the scope is configured for the repositories that will be used with gRPC
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddAutoMapper(typeof(BookingProfileGrpc));
builder.Services.AddAutoMapper(typeof(BookingProfile));

// the reason for adding the dbContext is the repositories depend on the details and the gRPC
// service will be using them to communicate with the database.
builder.Services.AddDbContext<BookingDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Booking-Api"));
});

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<BookingService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();