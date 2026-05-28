using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MemberProvisioningMiddleware
    {
        private readonly RequestDelegate _next;

        public MemberProvisioningMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DBContext dbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var subjectId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
                if (!string.IsNullOrEmpty(subjectId))
                {
                    var member = await dbContext.Members.FirstOrDefaultAsync(m => m.SubjectId == subjectId);
                    if (member == null)
                    {
                        var firstName = context.User.FindFirstValue(ClaimTypes.GivenName) ?? context.User.FindFirstValue("given_name") ?? "";
                        var lastName = context.User.FindFirstValue(ClaimTypes.Surname) ?? context.User.FindFirstValue("family_name") ?? "";
                        var fullName = $"{firstName} {lastName}".Trim();
                        var email = context.User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";

                        member = new Member
                        {
                            SubjectId = subjectId,
                            FullName = fullName,
                            Email = email,
                            JoinedDate = DateTime.UtcNow
                        };

                        dbContext.Members.Add(member);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MemberProvisioningMiddlewareExtensions
    {
        public static IApplicationBuilder UseMemberProvisioningMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MemberProvisioningMiddleware>();
        }
    }
}
