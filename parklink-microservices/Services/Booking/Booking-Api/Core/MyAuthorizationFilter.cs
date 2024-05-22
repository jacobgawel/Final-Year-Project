using Hangfire.Dashboard;

namespace Booking_Api.Core;

public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}