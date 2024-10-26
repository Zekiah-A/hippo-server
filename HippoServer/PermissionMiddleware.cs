using System.Text.RegularExpressions;
using HippoServer.DataModel;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

public partial class PermissionMiddleware
{
    private readonly RequestDelegate next;

    public PermissionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, DatabaseContext dbContext)
    {
        if (!context.Items.TryGetValue("Account", out var accountObj) || accountObj is not Account account)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var endpoint = context.GetEndpoint();
        var requiredPermissions = endpoint?.Metadata.GetMetadata<RequiredPermissionsMetadata>()?.Permissions;

        if (requiredPermissions == null || await HasRequiredPermissions(account, requiredPermissions, dbContext, context))
        {
            await next(context);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }

    private async Task<bool> HasRequiredPermissions(Account account, string[] requiredPermissions, DatabaseContext dbContext, HttpContext context)
    {
        var accountPermissions = account.Permissions.Select(p => p.Name).ToHashSet();
        
        // Group permissions are fetched only once
        var groupPermissions = await dbContext.Groups
            .Include(group => group.Members)
            .Include(group => group.Permissions)
            .Where(group => group.Members.Contains(account))
            .SelectMany(group => group.Permissions)
            .Select(permission => permission.Name)
            .ToListAsync();

        // Check required permissions against account and group permissions
        foreach (var permission in requiredPermissions)
        {
            if (accountPermissions.Contains(permission) || groupPermissions.Contains(permission))
            {
                return true;                
            }

            // Specific Resource ID-based permission check
            var resolvedPermission = ResolveIdPermission(context, permission);
            if (accountPermissions.Contains(resolvedPermission) || groupPermissions.Contains(resolvedPermission))
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveIdPermission(HttpContext context, string permissionTemplate)
    {
        var regex = IntegerIdTemplateRegex();
        return regex.Replace(permissionTemplate, match => context.Request.RouteValues[match.Groups[1].Value]?.ToString() ?? "*");
    }

    [GeneratedRegex(@"{(\d+)}")]
    private static partial Regex IntegerIdTemplateRegex();
}