using HippoServer.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

internal static partial class Program
{
    private static void MapAccountEndpoints()
    {
        app.MapGet("/accounts", async ([FromServices] DatabaseContext dbContext, HttpContext context) =>
        {
            return Results.Ok(await dbContext.Accounts.ToListAsync());
        });

        app.MapPost("/accounts/create", async (Account account, [FromServices] DatabaseContext dbContext, HttpContext context) =>
        {
            account.Created = DateTime.UtcNow;
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/accounts/{account.Id}", account);
        }).RequirePermissions("/accounts/create");

        app.MapDelete("/accounts/{id:int}", async (int id, [FromServices] DatabaseContext dbContext, HttpContext context) =>
        {
            var account = await dbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                return Results.NotFound();
            }

            dbContext.Accounts.Remove(account);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }).RequirePermissions("/accounts/*.delete", "/accounts/{0}.delete");

        app.MapPut("/accounts/{id:int}", async (int id, Account updatedAccount, [FromServices] DatabaseContext dbContext, HttpContext context) =>
        {
            var account = await dbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                return Results.NotFound();
            }

            account.FirstName = updatedAccount.FirstName;
            account.LastName = updatedAccount.LastName;
            account.Email = updatedAccount.Email;
            account.Balance = updatedAccount.Balance;
            account.Total = updatedAccount.Total;
            account.Activated = updatedAccount.Activated;
            await dbContext.SaveChangesAsync();

            return Results.Ok(account);
        }).RequirePermissions("/accounts/*.update", "/accounts/{0}.update");
    }
}