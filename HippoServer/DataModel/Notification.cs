namespace HippoServer.DataModel;

public class Notification
{
    public int Id { get; set; }
    public int CreatorId { get; set; }

    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public TimeSpan DurationHint { get; set;  }
    
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
}