using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using HippoServer.ApiModel;
using HippoServer.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

internal static partial class Program 
{
    private const string AlphabetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // TODO: Rate limit endpoints, add account create email verification
    private static void MapAuthEndpoints()
    {
        app.MapPost("/auth/create", async ([FromBody] AuthSignupRequest request, [FromServices] DatabaseContext dbContext, HttpContext httpContext) =>
        {
            if (await dbContext.Accounts.AnyAsync(account => account.Email == request.Email))
            {
                return Results.BadRequest(new ErrorResponse("Email already exists.", "auth.create.emailExists"));
            }

            // Update database
            var account = new Account
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Activated = null,
                Balance = 0,
                Total = 0,
                Token = GenToken()
            };
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
            var activationCode = GenCodeString();
            var activation = new Activation()
            {
                AccountId = account.Id,
                Code = activationCode,
                Created = DateTime.UtcNow
            };
            dbContext.Activations.Add(activation);
            await dbContext.SaveChangesAsync();
            
            // Send mail message
            // TODO: Ensure request host and path are not something modified, such as a proxy to protect against redirect URL injection
            var activationUrl = httpContext.Request.Protocol + httpContext.Request.Host.Value + 
                ":" + httpContext.Request.PathBase.Value + "/activate?email=" + account.Email + "&code=" + activationCode;
            var mailMessage = new MailMessage
            {
                From = new MailAddress(config!.SmtpFromEmail),
                Subject = "Hippo Casino Account Activation code",
                Body = 
                    $"""
                    <div style="border: 2px solid black;padding: 0px;font-family: Arial, sans-serif;display: flex;flex-direction: column;">
                        <header>
                            <h1 style="background-color: rgb(223, 128, 241);display: flex;flex-direction: row;align-items: center;column-gap: 8px;position: sticky;top: 0px;margin: 0px;left: 0px;padding: 8px;"><img src="https://raw.githubusercontent.com/Zekiah-A/hippo-web/refs/heads/main/assets/dripped-out-hippo-logo.webp" width="48" height="48">Hippo Casino: Activation Code</h1>
                        </header>
                        <main style="margin: 8px;font-size: 1.2em;flex-grow: 1;">
                            <h2>👋 Hello there</h2>
                            <p>Someone used your email to register a new <a href="https://hippo.casino" style="text-decoration: none;">hippo.casino</a> account.</p>
                            <p>If that's you, then cool, your account activation code is:</p>
                            <h1 style="background-color: #f7f7f7;display: inline;padding: 8px;border: 1px solid lightgray;">{activationCode}</h1>
                            <p>Or use this link to activate from another session <a href="{activationUrl}">{activationUrl}</a>.</p>
                            <p>Otherwise, you can ignore this email, we'll try not to message you again ❤️.</p>
                        </main>
                        <footer style="opacity: 0.6;margin-top: 16px;display: flex;flex-direction: row;padding: 16px;column-gap: 16px;background-color: rgb(175, 230, 211);">
                            <span>Email sent at {DateTime.UtcNow}</span>
                            <hr><span>Feel free to reply</span>
                            <hr><span>Contact <a href="mailto:{config.SmtpFromEmail}" style="text-decoration: none;">{config.SmtpFromEmail}</a></span>
                        </footer>
                    </div>
                    """,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(account.Email);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem("Failed to send verification email");
            }
        });

        app.MapPost("/auth/activate", async (AuthActivateRequest request, [FromServices] DatabaseContext dbContext, HttpContext httpContext) =>
        {
            throw new NotImplementedException();
        });

        app.MapPost("/auth/login", async ([FromBody] AuthLoginRequest request, [FromServices] DatabaseContext dbContext, HttpContext httpContext) =>
        {
            var account = await dbContext.Accounts.FirstOrDefaultAsync(account =>
                account.Email == request.Email);
            if (account is null)
            {
                return Results.NotFound(new ErrorResponse("Account not found", "auth.login.notFound"));
            }
            if (account.Activated is null)
            {
                // TODO: If activation has expired, then delete account
                return Results.BadRequest(new ErrorResponse("Account not activated.", "auth.login.notActivated"));
            }

            var verificationToken = GenToken();
            var verificationCode = GenCodeString();
            var verification = new Verification()
            {
                Code = verificationCode,
                Token = verificationToken,
                AccountId = account.Id,
                Created = DateTime.UtcNow
            };
            dbContext.Verifications.Add(verification);
            await dbContext.SaveChangesAsync();
            httpContext.Session.SetString("VerificationToken", verificationToken);
            
            // Send mail message
            var mailMessage = new MailMessage
            {
                From = new MailAddress(config.SmtpFromEmail),
                Subject = "Hippo Casino Account Verification code",
                Body = 
                    $"""
                    <div style="border: 2px solid black;padding: 0px;font-family: Arial, sans-serif;display: flex;flex-direction: column;">
                        <header>
                            <h1 style="background-color: rgb(223, 128, 241);display: flex;flex-direction: row;align-items: center;column-gap: 8px;position: sticky;top: 0px;margin: 0px;left: 0px;padding: 8px;"><img src="https://raw.githubusercontent.com/Zekiah-A/hippo-web/refs/heads/main/assets/dripped-out-hippo-logo.webp" width="48" height="48">Hippo Casino: Verification Code</h1>
                        </header>
                        <main style="margin: 8px;font-size: 1.2em;flex-grow: 1;">
                            <h2>👋 Hello {account.FirstName}!</h2>
                            <p>Here is the login verification code for your <a href="https://hippo.casino" style="text-decoration: none;">hippo.casino</a> account.</p>
                            <p>If it was you who tried to log in, then cool, your code is:</p>
                            <h1 style="background-color: #f7f7f7;display: inline;padding: 8px;border: 1px solid lightgray;">{verificationCode}</h1>
                            <p>Otherwise, you can ignore this email, and your account will remain secure.</p>
                        </main>
                        <footer style="opacity: 0.6;margin-top: 16px;display: flex;flex-direction: row;padding: 16px;column-gap: 16px;background-color: rgb(175, 230, 211);">
                            <span>Email sent at {DateTime.UtcNow}</span>
                            <hr><span>Feel free to reply</span>
                            <hr><span>Contact <a href="mailto:{config.SmtpFromEmail}" style="text-decoration: none;">{config.SmtpFromEmail}</a></span>
                        </footer>
                    </div>
                    """,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(account.Email);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem("Failed to send verification email");
            }
        });

        app.MapPost("/auth/verify", async ([FromBody]AuthVerificationRequest request, [FromServices] DatabaseContext dbContext, HttpContext httpContext) =>
        {
            var verificationToken = httpContext.Session.GetString("VerificationToken");
            var verification = await dbContext.Verifications
                .Include(verification => verification.Account)
                .FirstOrDefaultAsync(verification => verification.Token == verificationToken);
            if (verification == null || DateTime.UtcNow - verification.Created > TimeSpan.FromMinutes(15))
            {
                return Results.NotFound(new ErrorResponse("Specified verification is expired or doesn't exist.", "auth.verify.notFound"));
            }
            if (verification.Code != request.Code)
            {
                return Results.BadRequest(new ErrorResponse("Specified verification code was invalid.", "auth.verify.invalidCode"));
            }

            // DB Updates
            var account = verification.Account;
            if (account.Activated != null)
            {
                return Results.NotFound(new ErrorResponse("Account not found or not activated.", "auth.verify.notFound"));
            }
            dbContext.Verifications.Remove(verification);
            await dbContext.SaveChangesAsync();
            
            // Response
            httpContext.Session.SetString("Token", account.Token);
            return Results.Ok(new { Token = account.Token });
        });

        app.MapPost("/auth/logout", (HttpContext httpContext) =>
        {
            httpContext.Session.Clear();
            return Results.Ok();
        });
    }

    /// <summary>
    /// Will generate a random code string, that looks like '[letter][number][number]-[letter][number][number]',
    /// for example F12-E32, or Z11-G44. Giving 6,760,000 million possible code combinations.
    /// </summary>
    private static string GenCodeString()
    {
        var codeBuilder = new StringBuilder();
        var alphaNumeric1 = AlphabetUpper[RandomNumberGenerator.GetInt32(0, AlphabetUpper.Length)];
        codeBuilder.Append(alphaNumeric1);
        var numeric1 = RandomNumberGenerator.GetInt32(10, 100);
        codeBuilder.Append(numeric1);
        // Separator
        codeBuilder.Append('-');
        var alphaNumeric2 = AlphabetUpper[RandomNumberGenerator.GetInt32(0, AlphabetUpper.Length)];
        codeBuilder.Append(alphaNumeric2);
        var numeric2 = RandomNumberGenerator.GetInt32(10, 100).ToString();
        codeBuilder.Append(numeric2);
        return codeBuilder.ToString();
    }

    private static string GenToken()
    {
        return RandomNumberGenerator.GetHexString(32);
    }
}