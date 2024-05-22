using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Parking_Infrastructure.Data;

namespace Parking_Api.Core;

public class DbInitializer
{
    public static void InitDb(WebApplication app)
    {
        var retry = 10;

        while (retry > 0)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<ParkingDbContext>();
                dbContext?.Database.Migrate();
                dbContext?.Database.EnsureCreated();
                break;
            }
            catch (SqlException e)
            {
                retry -= 1;
                Console.WriteLine("SQL Server is not ready. Attempting to connect in 5 secs. Retries left: " + retry);
                Thread.Sleep(5000);
            }
        }
    }
}