namespace HippoServer.DataModel;

public class Event
{
    public int Id { get; set; }
    
    // TimeDescription - Human readable transcription of time info
    public string? TimeDescription { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime FinishTime { get; set; }
    
    // TypeDescription - Human readable transcription of time info
    public string? TypeDescription { get; set; }
    public EventType Type { get; set; }

    public DateTime Created { get; set; }
}