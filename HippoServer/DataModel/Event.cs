namespace HippoServer.DataModel;

public class Event
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime FinishTime { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public DateTime Created { get; set; }
}