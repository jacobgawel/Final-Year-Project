using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Reviews_Api.Core;
using Reviews_Infrastructure.Persistence.Data;
using Reviews_Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add authentication to the resource
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddDbContext<ReviewDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("Reviews-Api"));
});

builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// CORS POLICY

// const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

/*builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        build =>
        {
            build.WithOrigins().AllowAnyOrigin();
        });
});*/

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// CORS POLICY 
// app.UseCors(myAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();