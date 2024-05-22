using Hangfire.Dashboard;

namespace Fines_Api.Core;

public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}