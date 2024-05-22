using System.Security.Claims;

namespace Booking_Tests.Utils
{
    public class Helpers
    {
        public static ClaimsPrincipal GetClaimsPrincipal()
        {

            var claims = new List<Claim>
            {
                new Claim("username", "test"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.NameIdentifier, "025afb7c-5483-4abe-b17e-0b3dde9eb75b")
            };
            var identity = new ClaimsIdentity(claims, "testing");
            return new ClaimsPrincipal(identity);
        }
    }
}
