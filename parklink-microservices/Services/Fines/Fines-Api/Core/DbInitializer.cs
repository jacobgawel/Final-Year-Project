using Fines_Infrastructure.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Fines_Api.Core;

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
                var dbContext = scope.ServiceProvider.GetService<FineDbContext>();
                dbContext?.Database.Migrate();
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