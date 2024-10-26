using HippoServer.DataModel;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DatabaseContext dbContext)
    {
        if (context.Session.TryGetValue("Token", out var tokenBytes))
        {
            var token = System.Text.Encoding.UTF8.GetString(tokenBytes);
            var account = await dbContext.Accounts
                .Include(account => account.Permissions)
                .Include(account => account.Groups)
                .ThenInclude(group => group.Permissions)
                .FirstOrDefaultAsync(account => account.Token == token);

            if (account != null)
            {
                context.Items["Account"] = account;
            }
        }

        await _next(context);
    }
}