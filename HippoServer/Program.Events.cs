using HippoServer.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

internal static partial class Program
{
    private static void MapEventEndpoints()
    {
        // Create Event
        app.MapPost("/events/create", async ([FromBody] Event newEvent, [FromServices] DatabaseContext dbContext) =>
        {
            // TODO: Add authentication
            throw new NotImplementedException();
            dbContext.Events.Add(newEvent);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/events/{newEvent.Id}", newEvent);
        });

        // Retrieve Events with filters
        app.MapGet("/events", async ([FromQuery] string? type, [FromQuery] DateTime? before, [FromQuery] int? count, [FromServices] DatabaseContext dbContext) =>
        {
            var query = dbContext.Events.AsQueryable();

            // Filter by event type if specified
            if (Enum.TryParse(type, out EventType eventType))
            {
                query = query.Where(e => e.Type == eventType);
            }

            // Filter by date if 'before' parameter is specified
            if (before.HasValue)
            {
                query = query.Where(e => e.StartTime < before.Value);
            }

            // Limit result count if specified
            if (count.HasValue && count > 0)
            {
                query = query.Take(count.Value);
            }

            var events = await query.ToListAsync();
            return Results.Ok(events);
        });

        // Retrieve Event by ID
        app.MapGet("/events/{id:int}", async (int id, [FromServices] DatabaseContext dbContext) =>
        {
            // TODO: Add authentication
            throw new NotImplementedException();
            var eventItem = await dbContext.Events.FindAsync(id);
            return eventItem == null ? Results.NotFound() : Results.Ok(eventItem);
        });

        // Update Event by ID
        app.MapPut("/events/{id:int}", async (int id, [FromBody] Event updatedEvent, [FromServices] DatabaseContext dbContext) =>
        {
            // TODO: Add authentication
            throw new NotImplementedException();
            var existingEvent = await dbContext.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return Results.NotFound();
            }

            existingEvent.TimeDescription = updatedEvent.TimeDescription;
            existingEvent.StartTime = updatedEvent.StartTime;
            existingEvent.Duration = updatedEvent.Duration;
            existingEvent.FinishTime = updatedEvent.FinishTime;
            existingEvent.TypeDescription = updatedEvent.TypeDescription;
            existingEvent.Type = updatedEvent.Type;

            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        // Delete Event by ID
        app.MapDelete("/events/{id:int}", async (int id, [FromServices] DatabaseContext dbContext) =>
        {
            // TODO: Add authentication
            throw new NotImplementedException();
            var eventItem = await dbContext.Events.FindAsync(id);
            if (eventItem == null)
            {
                return Results.NotFound();
            }

            dbContext.Events.Remove(eventItem);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}